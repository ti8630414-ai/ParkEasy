using System;
using System.ComponentModel.DataAnnotations;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.ViewModels
{
    public class PaymentSimulatorViewModel
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string ParkingName { get; set; } = string.Empty;
        public string SlotNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public decimal Amount { get; set; }

        // Simulation parameters
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Bkash;

        [Display(Name = "Mobile Number (01XXXXXXXXX)")]
        public string? WalletAccountNumber { get; set; } = "01712345678";

        [Display(Name = "Bank Account No. (Demo)")]
        public string? BankAccountNumber { get; set; } = "1234567890";

        [Display(Name = "Bank")]
        public string? BankName { get; set; } = "UCB";

        public static readonly List<string> BangladeshBanks = new()
        {
            "UCB", "City Bank", "Sonali Bank", "Dutch-Bangla Bank", "BRAC Bank",
            "Eastern Bank", "Pubali Bank", "Islami Bank", "Janata Bank", "Agrani Bank"
        };

        [Display(Name = "Cardholder Name")]
        public string CardHolderName { get; set; } = "John Doe";

        [Display(Name = "Card Number (Demo)")]
        public string CardNumber { get; set; } = "4242 •••• •••• 4242";

        [Display(Name = "Expiry (MM/YY)")]
        public string ExpiryDate { get; set; } = "12/28";

        [Display(Name = "CVV / CVC")]
        public string Cvv { get; set; } = "123";

        [Display(Name = "Mobile Wallet Provider")]
        public string? MobileWalletProvider { get; set; } = "PayPal / Demo Pay";

        [Display(Name = "Simulate Outcome")]
        public bool SimulateSuccess { get; set; } = true;
    }

    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class RefundProcessViewModel
    {
        [Required]
        public int BookingId { get; set; }

        public int PaymentId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalPaidAmount { get; set; }

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Refund amount must be greater than 0.")]
        [Display(Name = "Refund Amount (৳)")]
        public decimal RefundAmount { get; set; }

        [Required(ErrorMessage = "Please enter reason for refund.")]
        [StringLength(500)]
        [Display(Name = "Refund Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Admin / Owner Note")]
        public string? AdminNote { get; set; }
    }
}
