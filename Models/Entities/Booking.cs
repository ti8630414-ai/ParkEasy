using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string BookingReference { get; set; } = string.Empty; // e.g. PE-2026-XXXX

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int ParkingSpaceId { get; set; }

        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace ParkingSpace { get; set; } = null!;

        [Required]
        public int ParkingSlotId { get; set; }

        [ForeignKey("ParkingSlotId")]
        public virtual ParkingSlot ParkingSlot { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string VehiclePlateNumber { get; set; } = string.Empty;

        public VehicleType VehicleType { get; set; } = VehicleType.Car;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public double DurationHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Requested;

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Payment? Payment { get; set; }
        public virtual Refund? Refund { get; set; }
    }
}
