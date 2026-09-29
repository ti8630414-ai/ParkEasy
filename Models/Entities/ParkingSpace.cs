using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkEasy.Web.Models.Entities
{
    public class ParkingSpace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000)]
        public decimal BasePricePerHour { get; set; }

        public TimeSpan OpeningTime { get; set; } = new TimeSpan(0, 0, 0); // 00:00
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(23, 59, 59); // 23:59
        public bool Is24Hours { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Amenities
        public bool HasCCTV { get; set; } = true;
        public bool HasEVCharging { get; set; } = false;
        public bool HasCoveredParking { get; set; } = false;
        public bool HasDisabledAccess { get; set; } = false;
        public bool HasValet { get; set; } = false;
        public bool HasSecurityGuard { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ParkingSlot> Slots { get; set; } = new List<ParkingSlot>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
