using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.Entities
{
    public class Refund
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; } = null!;

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        public string? ProcessedByUserId { get; set; }

        [ForeignKey("ProcessedByUserId")]
        public virtual ApplicationUser? ProcessedByUser { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(500)]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
