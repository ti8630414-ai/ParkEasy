using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Data;
using ParkEasy.Web.Helpers;
using ParkEasy.Web.Hubs;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IEmailSimulationService _emailService;
        private readonly IHubContext<ParkingHub> _hubContext;

        public BookingService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IQrCodeService qrCodeService,
            IEmailSimulationService emailService,
            IHubContext<ParkingHub> hubContext)
        {
            _context = context;
            _notificationService = notificationService;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        public async Task<bool> IsSlotAvailableAsync(int slotId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null || !slot.IsActive || slot.CurrentStatus == SlotStatus.Maintenance)
                return false;

            var startUtc = DateTimeHelper.ToUtc(startTime);
            var endUtc = DateTimeHelper.ToUtc(endTime);

            // Check for overlapping active/confirmed/requested bookings
            var query = _context.Bookings
                .Where(b => b.ParkingSlotId == slotId &&
                            (b.Status == BookingStatus.Requested || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                            b.StartTime < endUtc && b.EndTime > startUtc);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<(bool Success, string Message, Booking? Booking)> CreateBookingAsync(CreateBookingViewModel model, string userId)
        {
            var slot = await _context.ParkingSlots
                .Include(s => s.ParkingSpace)
                .FirstOrDefaultAsync(s => s.Id == model.ParkingSlotId);

            if (slot == null || !slot.IsActive)
                return (false, "Selected parking slot is invalid or inactive.", null);

            var startDateTime = DateTimeHelper.CombineToUtc(model.BookingDate, model.StartTime);
            var endDateTime = startDateTime.AddHours(model.DurationHours);

            if (startDateTime < DateTime.UtcNow.AddMinutes(-10))
                return (false, "Booking start time cannot be in the past.", null);

            if (model.DurationHours <= 0)
                return (false, "Booking duration must be at least 1 hour.", null);

            // Concurrency-safe check with transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool isAvailable = await IsSlotAvailableAsync(slot.Id, startDateTime, endDateTime);
                if (!isAvailable)
                {
                    return (false, "Sorry, this slot has just been reserved for the selected time window. Please choose another slot.", null);
                }

                // Generate unique booking reference
                var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
                var bookingRef = $"PE-{DateTime.UtcNow:yyyyMMdd}-{randomSuffix}";

                var pricePerHour = slot.PricePerHour > 0 ? slot.PricePerHour : slot.ParkingSpace.BasePricePerHour;
                var totalAmount = pricePerHour * (decimal)model.DurationHours;

                var booking = new Booking
                {
                    BookingReference = bookingRef,
                    UserId = userId,
                    ParkingSpaceId = slot.ParkingSpaceId,
                    ParkingSlotId = slot.Id,
                    VehiclePlateNumber = model.VehiclePlateNumber.Trim().ToUpper(),
                    VehicleType = model.VehicleType,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    DurationHours = model.DurationHours,
                    PricePerHour = pricePerHour,
                    TotalAmount = totalAmount,
                    Status = BookingStatus.Confirmed, // Automatic confirmation for instant booking flow
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send notification to User
                await _notificationService.CreateNotificationAsync(
                    userId,
                    "Booking Created Successfully!",
                    $"Your booking {booking.BookingReference} at {slot.ParkingSpace.Name} (Slot {slot.SlotNumber}) is confirmed.",
                    NotificationType.BookingCreated,
                    $"/Booking/Details/{booking.Id}");

                // Send notification to Parking Owner
                await _notificationService.CreateNotificationAsync(
                    slot.ParkingSpace.OwnerId,
                    "New Parking Booking Received",
                    $"Slot {slot.SlotNumber} at {slot.ParkingSpace.Name} has been booked for {booking.StartTime:MMM dd, HH:mm}.",
                    NotificationType.BookingCreated,
                    $"/Owner/Requests");

                // Broadcast real-time SignalR slot update
                try
                {
                    await _hubContext.Clients.Group($"Space_{slot.ParkingSpaceId}").SendAsync(
                        "SlotStatusChanged",
                        slot.ParkingSpaceId,
                        slot.Id,
                        SlotStatus.Occupied.ToString(),
                        false);
                }
                catch { }

                return (true, "Booking created successfully!", booking);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"An error occurred while creating booking: {ex.Message}", null);
            }
        }

        public async Task<BookingDetailsViewModel?> GetBookingDetailsAsync(int id, string? currentUserId = null, bool isElevated = false)
        {
            var b = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .Include(b => b.Refund)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (b == null) return null;

            // Security check: User must own booking, or own the parking space, or be Admin
            if (!isElevated && currentUserId != null && b.UserId != currentUserId && b.ParkingSpace.OwnerId != currentUserId)
                return null;

            // Generate QR Code payload
            var qrPayload = $"PARKEASY|REF:{b.BookingReference}|SLOT:{b.ParkingSlot.SlotNumber}|START:{b.StartTime:s}|END:{b.EndTime:s}|VEHICLE:{b.VehiclePlateNumber}|STATUS:{b.Status}";
            var qrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(qrPayload);

            return new BookingDetailsViewModel
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                UserId = b.UserId,
                UserName = b.User.FullName ?? b.User.UserName ?? "User",
                UserEmail = b.User.Email ?? "N/A",
                UserPhone = b.User.PhoneNumber,
                ParkingSpaceId = b.ParkingSpaceId,
                ParkingSpaceName = b.ParkingSpace.Name,
                ParkingSpaceAddress = b.ParkingSpace.Address,
                ParkingSpaceCity = b.ParkingSpace.City,
                Latitude = b.ParkingSpace.Latitude,
                Longitude = b.ParkingSpace.Longitude,
                ParkingSlotId = b.ParkingSlotId,
                SlotNumber = b.ParkingSlot.SlotNumber,
                FloorOrZone = b.ParkingSlot.FloorOrZone,
                VehiclePlateNumber = b.VehiclePlateNumber,
                VehicleType = b.VehicleType,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                DurationHours = b.DurationHours,
                PricePerHour = b.PricePerHour,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                RejectionReason = b.RejectionReason,
                CancellationReason = b.CancellationReason,
                CancelledAt = b.CancelledAt,
                CreatedAt = b.CreatedAt,
                PaymentId = b.Payment?.Id,
                TransactionId = b.Payment?.TransactionId,
                PaymentMethod = b.Payment?.PaymentMethod,
                PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
                PaidAt = b.Payment?.PaidAt,
                RefundId = b.Refund?.Id,
                RefundAmount = b.Refund?.Amount,
                RefundStatus = b.Refund?.Status,
                RefundReason = b.Refund?.Reason,
                QrCodeBase64 = qrCodeBase64
            };
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .Include(b => b.Refund)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BookingHistoryViewModel> GetUserBookingsAsync(string userId, string filter = "all")
        {
            var query = _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            var allBookings = await query.ToListAsync();
            var now = DateTime.UtcNow;

            var summaries = allBookings.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                ParkingName = b.ParkingSpace.Name,
                ParkingAddress = b.ParkingSpace.Address,
                ParkingCity = b.ParkingSpace.City,
                SlotNumber = b.ParkingSlot.SlotNumber,
                FloorOrZone = b.ParkingSlot.FloorOrZone,
                VehiclePlateNumber = b.VehiclePlateNumber,
                VehicleType = b.VehicleType,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                DurationHours = b.DurationHours,
                PricePerHour = b.PricePerHour,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
                TransactionId = b.Payment?.TransactionId,
                CreatedAt = b.CreatedAt
            }).ToList();

            var filtered = filter.ToLower() switch
            {
                "upcoming" => summaries.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Requested) && b.StartTime > now).ToList(),
                "active" => summaries.Where(b => b.Status == BookingStatus.Active || ((b.Status == BookingStatus.Confirmed) && b.StartTime <= now && b.EndTime >= now)).ToList(),
                "completed" => summaries.Where(b => b.Status == BookingStatus.Completed || ((b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) && b.EndTime < now)).ToList(),
                "cancelled" => summaries.Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected).ToList(),
                _ => summaries
            };

            return new BookingHistoryViewModel
            {
                Filter = filter,
                Bookings = filtered,
                UpcomingCount = summaries.Count(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Requested) && b.StartTime > now),
                ActiveCount = summaries.Count(b => b.Status == BookingStatus.Active || ((b.Status == BookingStatus.Confirmed) && b.StartTime <= now && b.EndTime >= now)),
                CompletedCount = summaries.Count(b => b.Status == BookingStatus.Completed || ((b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) && b.EndTime < now)),
                CancelledCount = summaries.Count(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Rejected)
            };
        }

        public async Task<List<BookingSummaryViewModel>> GetBookingsByOwnerIdAsync(string ownerId, string? status = null)
        {
            var query = _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .Include(b => b.User)
                .Where(b => b.ParkingSpace.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }

            var list = await query.ToListAsync();

            return list.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                ParkingName = b.ParkingSpace.Name,
                ParkingAddress = b.ParkingSpace.Address,
                ParkingCity = b.ParkingSpace.City,
                SlotNumber = b.ParkingSlot.SlotNumber,
                FloorOrZone = b.ParkingSlot.FloorOrZone,
                VehiclePlateNumber = b.VehiclePlateNumber,
                VehicleType = b.VehicleType,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                DurationHours = b.DurationHours,
                PricePerHour = b.PricePerHour,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
                TransactionId = b.Payment?.TransactionId,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public async Task<List<BookingSummaryViewModel>> GetAllBookingsAsync(string? status = null)
        {
            var query = _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }

            var list = await query.ToListAsync();

            return list.Select(b => new BookingSummaryViewModel
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                ParkingName = b.ParkingSpace.Name,
                ParkingAddress = b.ParkingSpace.Address,
                ParkingCity = b.ParkingSpace.City,
                SlotNumber = b.ParkingSlot.SlotNumber,
                FloorOrZone = b.ParkingSlot.FloorOrZone,
                VehiclePlateNumber = b.VehiclePlateNumber,
                VehicleType = b.VehicleType,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                DurationHours = b.DurationHours,
                PricePerHour = b.PricePerHour,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
                TransactionId = b.Payment?.TransactionId,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public async Task<DigitalPassViewModel?> GetDigitalPassAsync(int bookingId, string currentUserId, bool isElevated = false)
        {
            var b = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (b == null) return null;

            if (!isElevated && b.UserId != currentUserId && b.ParkingSpace.OwnerId != currentUserId)
                return null;

            var qrPayload = $"PARKEASY-PASS|ID:{b.BookingReference}|USER:{b.User.FullName}|SLOT:{b.ParkingSlot.SlotNumber}|LOC:{b.ParkingSpace.Name}|TIME:{b.StartTime:yyyy-MM-dd HH:mm} to {b.EndTime:yyyy-MM-dd HH:mm}|VEHICLE:{b.VehiclePlateNumber}";
            var qrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(qrPayload);

            return new DigitalPassViewModel
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                UserName = b.User.FullName ?? b.User.UserName ?? "Customer",
                UserEmail = b.User.Email ?? "N/A",
                UserPhone = b.User.PhoneNumber ?? "N/A",
                ParkingName = b.ParkingSpace.Name,
                ParkingAddress = b.ParkingSpace.Address,
                ParkingCity = b.ParkingSpace.City,
                SlotNumber = b.ParkingSlot.SlotNumber,
                FloorOrZone = b.ParkingSlot.FloorOrZone,
                VehiclePlateNumber = b.VehiclePlateNumber,
                VehicleType = b.VehicleType,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                DurationHours = b.DurationHours,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
                TransactionId = b.Payment?.TransactionId,
                QrCodeBase64 = qrCodeBase64,
                IssuedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> ApproveBookingAsync(int bookingId, string currentUserId, bool isAdmin = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;
            if (!isAdmin && booking.ParkingSpace.OwnerId != currentUserId) return false;

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Booking Approved!",
                $"Your booking {booking.BookingReference} has been approved by the parking owner.",
                NotificationType.BookingApproved,
                $"/Booking/Details/{booking.Id}");

            return true;
        }

        public async Task<bool> RejectBookingAsync(int bookingId, string reason, string currentUserId, bool isAdmin = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;
            if (!isAdmin && booking.ParkingSpace.OwnerId != currentUserId) return false;

            booking.Status = BookingStatus.Rejected;
            booking.RejectionReason = reason;
            booking.UpdatedAt = DateTime.UtcNow;

            // If payment was made, initiate refund record
            if (booking.Payment != null && booking.Payment.Status == PaymentStatus.Paid)
            {
                var refund = new Refund
                {
                    BookingId = booking.Id,
                    PaymentId = booking.Payment.Id,
                    Amount = booking.Payment.Amount,
                    Reason = $"Booking rejected by owner: {reason}",
                    Status = RefundStatus.Approved,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };
                booking.Payment.Status = PaymentStatus.Refunded;
                _context.Refunds.Add(refund);
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Booking Rejected",
                $"Your booking {booking.BookingReference} was rejected. Reason: {reason}",
                NotificationType.BookingRejected,
                $"/Booking/Details/{booking.Id}");

            return true;
        }

        public async Task<bool> CheckInBookingAsync(int bookingId, string currentUserId, bool isElevated = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.ParkingSpace)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;
            if (!isElevated && booking.UserId != currentUserId && booking.ParkingSpace.OwnerId != currentUserId) return false;

            booking.Status = BookingStatus.Active;
            booking.CheckedInAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Checked In Successfully",
                $"You have checked in for booking {booking.BookingReference}. Enjoy your parking!",
                NotificationType.SystemAlert,
                $"/Booking/Details/{booking.Id}");

            return true;
        }

        public async Task<bool> CompleteBookingAsync(int bookingId, string currentUserId, bool isElevated = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.ParkingSpace)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return false;
            if (!isElevated && booking.UserId != currentUserId && booking.ParkingSpace.OwnerId != currentUserId) return false;

            booking.Status = BookingStatus.Completed;
            booking.CheckedOutAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Booking Completed",
                $"Thank you for using ParkEasy! Booking {booking.BookingReference} is completed.",
                NotificationType.SystemAlert,
                $"/Booking/Details/{booking.Id}");

            return true;
        }

        public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string reason, string currentUserId, bool isAdmin = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, "Booking not found.");

            if (!isAdmin && booking.UserId != currentUserId && booking.ParkingSpace.OwnerId != currentUserId)
                return (false, "Unauthorized to cancel this booking.");

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                return (false, "This booking has already been finalised.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = reason;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            // Handle refund if paid
            if (booking.Payment != null && booking.Payment.Status == PaymentStatus.Paid)
            {
                var refund = new Refund
                {
                    BookingId = booking.Id,
                    PaymentId = booking.Payment.Id,
                    Amount = booking.Payment.Amount,
                    Reason = $"Booking cancelled: {reason}",
                    Status = RefundStatus.Approved,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };
                booking.Payment.Status = PaymentStatus.Refunded;
                _context.Refunds.Add(refund);
            }

            await _context.SaveChangesAsync();

            // Notify User
            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Booking Cancelled",
                $"Booking {booking.BookingReference} has been cancelled successfully.",
                NotificationType.BookingCancelled,
                $"/Booking/Details/{booking.Id}");

            // Notify Space Owner
            await _notificationService.CreateNotificationAsync(
                booking.ParkingSpace.OwnerId,
                "Booking Cancelled by User",
                $"Booking {booking.BookingReference} for slot {booking.ParkingSlot.SlotNumber} has been cancelled.",
                NotificationType.BookingCancelled,
                $"/Owner/Requests");

            return (true, "Booking cancelled successfully.");
        }

        public async Task<int> ProcessExpiredAndCompletedBookingsAsync()
        {
            var now = DateTime.UtcNow;
            var pastBookings = await _context.Bookings
                .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) && b.EndTime < now)
                .ToListAsync();

            foreach (var b in pastBookings)
            {
                b.Status = BookingStatus.Completed;
                b.UpdatedAt = now;
            }

            if (pastBookings.Any())
            {
                await _context.SaveChangesAsync();
            }

            return pastBookings.Count;
        }
    }
}
