using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.ViewModels
{
    public class ParkingSearchViewModel
    {
        public string? Query { get; set; }
        public string? City { get; set; }
        public VehicleType? VehicleType { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double RadiusKm { get; set; } = 15.0; // Default 15km search

        public bool HasCCTV { get; set; }
        public bool HasEVCharging { get; set; }
        public bool HasCoveredParking { get; set; }
        public bool HasDisabledAccess { get; set; }
        public bool HasValet { get; set; }

        public string SortBy { get; set; } = "price_asc"; // price_asc, price_desc, distance, rating

        public List<ParkingSpaceCardViewModel> Results { get; set; } = new List<ParkingSpaceCardViewModel>();
        public List<string> AvailableCities { get; set; } = new List<string>();
        public int TotalCount => Results.Count;
    }

    public class ParkingSpaceCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal BasePricePerHour { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public double? DistanceKm { get; set; }
        public bool HasCCTV { get; set; }
        public bool HasEVCharging { get; set; }
        public bool HasCoveredParking { get; set; }
        public bool HasDisabledAccess { get; set; }
        public bool HasValet { get; set; }
        public bool HasSecurityGuard { get; set; }
        public bool Is24Hours { get; set; }
        public string OperatingHours { get; set; } = "24/7";
    }

    public class ParkingSpaceDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal BasePricePerHour { get; set; }
        public string? ImageUrl { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;

        public bool Is24Hours { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasEVCharging { get; set; }
        public bool HasCoveredParking { get; set; }
        public bool HasDisabledAccess { get; set; }
        public bool HasValet { get; set; }
        public bool HasSecurityGuard { get; set; }

        public int TotalSlotsCount { get; set; }
        public int AvailableSlotsCount { get; set; }

        public List<SlotDetailDto> Slots { get; set; } = new List<SlotDetailDto>();

        // Pre-fill parameters for booking
        public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;
        public TimeSpan SelectedStartTime { get; set; } = DateTime.UtcNow.TimeOfDay.Add(TimeSpan.FromMinutes(30 - (DateTime.UtcNow.Minute % 30)));
        public int DurationHours { get; set; } = 2;
    }

    public class SlotDetailDto
    {
        public int Id { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = "Ground";
        public VehicleType SupportedVehicleType { get; set; }
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; }
        public SlotStatus CurrentStatus { get; set; }
        public bool HasEVCharger { get; set; }
        public bool IsAvailableForRequestedTime { get; set; } = true;
    }

    public class ParkingSpaceCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Parking Space Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Parking Space Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "State / Province")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        [Display(Name = "Latitude")]
        [Range(-90, 90, ErrorMessage = "Invalid Latitude.")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Display(Name = "Longitude")]
        [Range(-180, 180, ErrorMessage = "Invalid Longitude.")]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Base hourly rate is required.")]
        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000.")]
        [Display(Name = "Base Price Per Hour (৳)")]
        public decimal BasePricePerHour { get; set; }

        [Display(Name = "24/7 Operation")]
        public bool Is24Hours { get; set; } = true;

        [Display(Name = "Opening Time")]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(6, 0, 0);

        [Display(Name = "Closing Time")]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0);

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "CCTV Surveillance")]
        public bool HasCCTV { get; set; } = true;

        [Display(Name = "EV Charging Station")]
        public bool HasEVCharging { get; set; }

        [Display(Name = "Covered / Indoor Parking")]
        public bool HasCoveredParking { get; set; }

        [Display(Name = "Disabled Access (Accessible Parking)")]
        public bool HasDisabledAccess { get; set; }

        [Display(Name = "Valet Service")]
        public bool HasValet { get; set; }

        [Display(Name = "On-site Security Guard")]
        public bool HasSecurityGuard { get; set; } = true;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // Auto-generate slots on create
        [Display(Name = "Auto-Generate Slots Count")]
        [Range(0, 100, ErrorMessage = "Auto generated slots count must be between 0 and 100.")]
        public int AutoGenerateSlotsCount { get; set; } = 3;

        [Display(Name = "Default Zone / Floor")]
        public string DefaultZone { get; set; } = "Ground Level";
    }

    public class SlotCreateEditViewModel
    {
        public int Id { get; set; }

        public int ParkingSpaceId { get; set; }
        public string ParkingSpaceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slot number is required.")]
        [StringLength(20)]
        [Display(Name = "Slot Number (e.g. A-01, G-12)")]
        public string SlotNumber { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Floor or Zone (e.g. Ground, B1, Floor 2)")]
        public string FloorOrZone { get; set; } = "Ground";

        [Required]
        [Display(Name = "Vehicle Type Supported")]
        public VehicleType SupportedVehicleType { get; set; } = VehicleType.Car;

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Price Per Hour (৳)")]
        public decimal PricePerHour { get; set; }

        [Display(Name = "EV Charger Available at this slot")]
        public bool HasEVCharger { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Current Operational Status")]
        public SlotStatus CurrentStatus { get; set; } = SlotStatus.Available;
    }
}
