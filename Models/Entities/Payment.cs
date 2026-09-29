using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        [Required]
        [MaxLength(64)]
        public string TransactionId { get; set; } = string.Empty; // e.g. TXN-PE-879412

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(4)]
        public string? CardLast4 { get; set; }

        [MaxLength(50)]
        public string? MobileWalletProvider { get; set; } // e.g., bKash, PayPal, Apple Pay

        [MaxLength(60)]
        public string? BankName { get; set; } // e.g., UCB, City Bank, Sonali Bank (BankTransfer)

        [MaxLength(32)]
        public string? ProviderAccount { get; set; } // masked wallet/bank account, e.g. ***1234

        [MaxLength(1000)]
        public string? GatewayResponse { get; set; }

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Refund? Refund { get; set; }
    }
}
