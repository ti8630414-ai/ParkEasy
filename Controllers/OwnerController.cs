using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Data;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class OwnerController : Controller
    {
        private readonly IParkingService _parkingService;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OwnerController(
            IParkingService parkingService,
            IBookingService bookingService,
            IPaymentService paymentService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _parkingService = parkingService;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var spaces = isAdmin
                ? await _parkingService.GetAllSpacesAsync()
                : await _parkingService.GetSpacesByOwnerIdAsync(userId ?? string.Empty);

            var bookings = isAdmin
                ? await _bookingService.GetAllBookingsAsync()
                : await _bookingService.GetBookingsByOwnerIdAsync(userId ?? string.Empty);

            var now = DateTime.UtcNow;
            var today = DateTime.UtcNow.Date;

            var totalSlots = spaces.Sum(s => s.TotalSlots);
            var availableSlots = spaces.Sum(s => s.AvailableSlots);
            var occupiedSlots = Math.Max(0, totalSlots - availableSlots);
            var occupancyRate = totalSlots > 0 ? (double)occupiedSlots / totalSlots * 100.0 : 0.0;

            var totalEarnings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).Sum(b => b.TotalAmount);
            var todayEarnings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid && b.CreatedAt.Date == today).Sum(b => b.TotalAmount);

            var recentBookings = bookings.Take(8).ToList();
            var pendingList = bookings.Where(b => b.Status == BookingStatus.Requested).ToList();

            var monthlyEarnings = bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new MonthlyEarningDto
                {
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Amount = g.Sum(x => x.TotalAmount),
                    BookingsCount = g.Count()
                })
                .OrderByDescending(m => m.MonthName)
                .Take(6)
                .ToList();

            var model = new OwnerDashboardViewModel
            {
                TotalParkingSpaces = spaces.Count,
                TotalSlots = totalSlots,
                AvailableSlots = availableSlots,
                OccupiedSlots = occupiedSlots,
                TotalBookings = bookings.Count,
                PendingRequests = pendingList.Count,
                ActiveBookings = bookings.Count(b => b.Status == BookingStatus.Active),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                TotalEarnings = totalEarnings,
                TodayEarnings = todayEarnings,
                OccupancyRatePercent = Math.Round(occupancyRate, 1),
                Spaces = spaces,
                RecentBookings = recentBookings,
                PendingBookingsList = pendingList,
                MonthlyEarnings = monthlyEarnings
            };

            return View(model);
        }

        public async Task<IActionResult> Spaces()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var spaces = isAdmin
                ? await _parkingService.GetAllSpacesAsync()
                : await _parkingService.GetSpacesByOwnerIdAsync(userId ?? string.Empty);

            return View(spaces);
        }

        [HttpGet]
        public IActionResult CreateSpace()
        {
            return View(new ParkingSpaceCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpace(ParkingSpaceCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var space = await _parkingService.CreateParkingSpaceAsync(model, userId);
            TempData["SuccessMessage"] = $"Parking Space '{space.Name}' and {model.AutoGenerateSlotsCount} initial slots created successfully!";
            return RedirectToAction(nameof(Spaces));
        }

        [HttpGet]
        public async Task<IActionResult> EditSpace(int id)
        {
            var space = await _parkingService.GetParkingSpaceByIdAsync(id);
            if (space == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            if (!isAdmin && space.OwnerId != userId)
                return Forbid();

            var model = new ParkingSpaceCreateEditViewModel
            {
                Id = space.Id,
                Name = space.Name,
                Description = space.Description,
                Address = space.Address,
                City = space.City,
                State = space.State,
                PostalCode = space.PostalCode,
                Latitude = space.Latitude,
                Longitude = space.Longitude,
                BasePricePerHour = space.BasePricePerHour,
                Is24Hours = space.Is24Hours,
                OpeningTime = space.OpeningTime,
                ClosingTime = space.ClosingTime,
                ImageUrl = space.ImageUrl,
                HasCCTV = space.HasCCTV,
                HasEVCharging = space.HasEVCharging,
                HasCoveredParking = space.HasCoveredParking,
                HasDisabledAccess = space.HasDisabledAccess,
                HasValet = space.HasValet,
                HasSecurityGuard = space.HasSecurityGuard,
                IsActive = space.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpace(ParkingSpaceCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var success = await _parkingService.UpdateParkingSpaceAsync(model, userId ?? string.Empty, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = "Parking space details updated successfully.";
                return RedirectToAction(nameof(Spaces));
            }

            TempData["ErrorMessage"] = "Failed to update parking space.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpace(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var success = await _parkingService.DeleteParkingSpaceAsync(id, userId ?? string.Empty, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = "Parking space deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete parking space.";
            }

            return RedirectToAction(nameof(Spaces));
        }

        public async Task<IActionResult> Slots(int spaceId)
        {
            var space = await _parkingService.GetParkingSpaceByIdAsync(spaceId);
            if (space == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            if (!isAdmin && space.OwnerId != userId)
                return Forbid();

            var slots = await _parkingService.GetSlotsForSpaceAsync(spaceId);
            ViewBag.ParkingSpace = space;
            return View(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSlot(SlotCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid slot information.";
                return RedirectToAction(nameof(Slots), new { spaceId = model.ParkingSpaceId });
            }

            await _parkingService.AddSlotAsync(model);
            TempData["SuccessMessage"] = $"Slot {model.SlotNumber} added successfully.";
            return RedirectToAction(nameof(Slots), new { spaceId = model.ParkingSpaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSlot(SlotCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid slot data.";
                return RedirectToAction(nameof(Slots), new { spaceId = model.ParkingSpaceId });
            }

            var success = await _parkingService.UpdateSlotAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = $"Slot {model.SlotNumber} updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update slot.";
            }

            return RedirectToAction(nameof(Slots), new { spaceId = model.ParkingSpaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlot(int slotId, int spaceId)
        {
            var success = await _parkingService.DeleteSlotAsync(slotId);
            if (success)
            {
                TempData["SuccessMessage"] = "Slot deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete slot.";
            }

            return RedirectToAction(nameof(Slots), new { spaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSlot(int slotId, int spaceId, bool isActive)
        {
            await _parkingService.ToggleSlotStatusAsync(slotId, isActive);
            TempData["SuccessMessage"] = $"Slot status updated.";
            return RedirectToAction(nameof(Slots), new { spaceId });
        }

        public async Task<IActionResult> Requests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var bookings = isAdmin
                ? await _bookingService.GetAllBookingsAsync()
                : await _bookingService.GetBookingsByOwnerIdAsync(userId ?? string.Empty);

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var success = await _bookingService.ApproveBookingAsync(bookingId, userId ?? string.Empty, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not approve booking.";
            }

            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int bookingId, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var success = await _bookingService.RejectBookingAsync(bookingId, reason, userId ?? string.Empty, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking rejected and notification sent to user.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not reject booking.";
            }

            return RedirectToAction(nameof(Requests));
        }

        public async Task<IActionResult> Earnings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var bookings = isAdmin
                ? await _bookingService.GetAllBookingsAsync()
                : await _bookingService.GetBookingsByOwnerIdAsync(userId ?? string.Empty);

            var paidBookings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();
            ViewBag.TotalEarnings = paidBookings.Sum(b => b.TotalAmount);
            ViewBag.TotalPaidBookings = paidBookings.Count;

            return View(paidBookings);
        }

        public async Task<IActionResult> Occupancy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var spaces = isAdmin
                ? await _parkingService.GetAllSpacesAsync()
                : await _parkingService.GetSpacesByOwnerIdAsync(userId ?? string.Empty);

            return View(spaces);
        }

        public async Task<IActionResult> Refunds()
        {
            var refunds = await _paymentService.GetAllRefundsAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            if (!isAdmin)
            {
                refunds = refunds.Where(r => r.Booking.ParkingSpace.OwnerId == userId).ToList();
            }

            return View(refunds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRefund(RefundProcessViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var success = await _paymentService.ProcessRefundAsync(model, userId ?? string.Empty, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = "Refund processed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not process refund.";
            }

            return RedirectToAction(nameof(Refunds));
        }
    }
}
