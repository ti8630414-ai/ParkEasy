using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Data;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public PaymentService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<PaymentResultViewModel> ProcessPaymentSimulationAsync(PaymentSimulatorViewModel model, string currentUserId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.ParkingSpace)
                .Include(b => b.ParkingSlot)
                .FirstOrDefaultAsync(b => b.Id == model.BookingId);

            if (booking == null)
            {
                return new PaymentResultViewModel
                {
                    IsSuccess = false,
                    BookingId = model.BookingId,
                    Message = "Booking record was not found."
                };
            }

            var txnSuffix = RandomNumberGenerator.GetInt32(100000, 999999);
            var transactionId = $"TXN-PE-{DateTime.UtcNow:yyyyMMdd}-{txnSuffix}";

            if (model.SimulateSuccess)
            {
                var payment = booking.Payment ?? new Payment { BookingId = booking.Id };
                payment.TransactionId = transactionId;
                payment.Amount = booking.TotalAmount;
                payment.PaymentMethod = model.PaymentMethod;
                payment.Status = PaymentStatus.Paid;
                payment.CardLast4 = model.PaymentMethod == PaymentMethod.CreditCard || model.PaymentMethod == PaymentMethod.DebitCard ? "4242" : null;
                payment.MobileWalletProvider = model.PaymentMethod switch
                {
                    PaymentMethod.Bkash => "bKash",
                    PaymentMethod.Nagad => "Nagad",
                    PaymentMethod.Rocket => "Rocket",
                    PaymentMethod.MobileWallet => model.MobileWalletProvider,
                    _ => null
                };
                payment.BankName = model.PaymentMethod == PaymentMethod.BankTransfer ? model.BankName : null;
                payment.ProviderAccount = model.PaymentMethod switch
                {
                    PaymentMethod.Bkash or PaymentMethod.Nagad or PaymentMethod.Rocket => MaskAccount(model.WalletAccountNumber),
                    PaymentMethod.BankTransfer => MaskAccount(model.BankAccountNumber),
                    _ => null
                };
                payment.GatewayResponse = model.PaymentMethod switch
                {
                    PaymentMethod.Bkash => "SIMULATED_BKASH_APPROVED_200_OK",
                    PaymentMethod.Nagad => "SIMULATED_NAGAD_APPROVED_200_OK",
                    PaymentMethod.Rocket => "SIMULATED_ROCKET_APPROVED_200_OK",
                    PaymentMethod.BankTransfer => "SIMULATED_BANK_APPROVED_200_OK",
                    _ => "SIMULATED_APPROVED_200_OK"
                };
                payment.PaidAt = DateTime.UtcNow;

                if (booking.Payment == null)
                {
                    _context.Payments.Add(payment);
                }

                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send user payment confirmation notification
                await _notificationService.CreateNotificationAsync(
                    booking.UserId,
                    "Payment Successful!",
                    $"Payment of ৳{booking.TotalAmount:F2} for booking {booking.BookingReference} was processed successfully (Txn: {transactionId}).",
                    NotificationType.PaymentSuccess,
                    $"/Booking/Details/{booking.Id}");

                return new PaymentResultViewModel
                {
                    IsSuccess = true,
                    BookingId = booking.Id,
                    BookingReference = booking.BookingReference,
                    TransactionId = transactionId,
                    Amount = booking.TotalAmount,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = PaymentStatus.Paid,
                    Message = "Payment simulated successfully! Your booking is confirmed."
                };
            }
            else
            {
                var payment = booking.Payment ?? new Payment { BookingId = booking.Id };
                payment.TransactionId = transactionId;
                payment.Amount = booking.TotalAmount;
                payment.PaymentMethod = model.PaymentMethod;
                payment.Status = PaymentStatus.Failed;
                payment.GatewayResponse = "SIMULATED_DECLINED_INSUFFICIENT_FUNDS_402";

                if (booking.Payment == null)
                {
                    _context.Payments.Add(payment);
                }

                await _context.SaveChangesAsync();

                await _notificationService.CreateNotificationAsync(
                    booking.UserId,
                    "Payment Failed",
                    $"Payment simulation for booking {booking.BookingReference} failed. Please try again or choose another payment method.",
                    NotificationType.PaymentFailed,
                    $"/Payment/Simulate/{booking.Id}");

                return new PaymentResultViewModel
                {
                    IsSuccess = false,
                    BookingId = booking.Id,
                    BookingReference = booking.BookingReference,
                    TransactionId = transactionId,
                    Amount = booking.TotalAmount,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = PaymentStatus.Failed,
                    Message = "Payment declined by simulated provider. You can retry with another method."
                };
            }
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .Include(p => p.Refund)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.ParkingSpace)
                .Include(p => p.Refund)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Refund>> GetAllRefundsAsync()
        {
            return await _context.Refunds
                .Include(r => r.Booking)
                    .ThenInclude(b => b.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ParkingSpace)
                .Include(r => r.Payment)
                .Include(r => r.ProcessedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ProcessRefundAsync(RefundProcessViewModel model, string currentUserId, bool isAdmin = false)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.ParkingSpace)
                .FirstOrDefaultAsync(b => b.Id == model.BookingId);

            if (booking == null || booking.Payment == null)
                return false;

            if (!isAdmin && booking.ParkingSpace.OwnerId != currentUserId)
                return false;

            var refund = await _context.Refunds.FirstOrDefaultAsync(r => r.BookingId == model.BookingId)
                         ?? new Refund { BookingId = model.BookingId, PaymentId = booking.Payment.Id };

            refund.Amount = model.RefundAmount;
            refund.Reason = model.Reason;
            refund.AdminNote = model.AdminNote;
            refund.Status = RefundStatus.Processed;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.ProcessedByUserId = currentUserId;

            booking.Payment.Status = PaymentStatus.Refunded;
            booking.Status = BookingStatus.Cancelled;

            if (refund.Id == 0)
            {
                _context.Refunds.Add(refund);
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Refund Processed",
                $"A refund of ৳{model.RefundAmount:F2} for booking {booking.BookingReference} has been processed successfully.",
                NotificationType.RefundIssued,
                $"/Booking/Details/{booking.Id}");

            return true;
        }

        private static string? MaskAccount(string? account)
        {
            if (string.IsNullOrWhiteSpace(account)) return null;
            var digits = new string(account.Where(char.IsDigit).ToArray());
            if (digits.Length <= 4) return digits;
            return "***" + digits[^4..];
        }
    }
}
