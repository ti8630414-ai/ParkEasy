using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? VehiclePlateNumber { get; set; }
        public VehicleType? PreferredVehicleType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ParkingSpace> OwnedParkingSpaces { get; set; } = new List<ParkingSpace>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
