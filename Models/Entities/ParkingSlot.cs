using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.Entities
{
    public class ParkingSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParkingSpaceId { get; set; }

        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace ParkingSpace { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string SlotNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FloorOrZone { get; set; } = "Ground";

        public VehicleType SupportedVehicleType { get; set; } = VehicleType.Car;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000)]
        public decimal PricePerHour { get; set; }

        public bool IsActive { get; set; } = true;

        public SlotStatus CurrentStatus { get; set; } = SlotStatus.Available;

        public bool HasEVCharger { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
