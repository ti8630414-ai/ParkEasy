using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.ViewModels
{
    public class SlotAvailabilityRequestDto
    {
        public int ParkingSpaceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public VehicleType? VehicleType { get; set; }
    }

    public class SlotAvailabilityResultDto
    {
        public int SlotId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = string.Empty;
        public VehicleType SupportedVehicleType { get; set; }
        public decimal PricePerHour { get; set; }
        public bool HasEVCharger { get; set; }
        public bool IsAvailable { get; set; }
        public string ReasonIfNotAvailable { get; set; } = string.Empty;
    }

    public class CreateBookingViewModel
    {
        [Required]
        public int ParkingSpaceId { get; set; }

        public string ParkingSpaceName { get; set; } = string.Empty;
        public string ParkingSpaceAddress { get; set; } = string.Empty;
        public string ParkingSpaceCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a parking slot.")]
        public int ParkingSlotId { get; set; }

        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle Plate Number is required.")]
        [StringLength(20, ErrorMessage = "Plate number cannot exceed 20 characters.")]
        [Display(Name = "Vehicle Plate Number")]
        public string VehiclePlateNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vehicle Type")]
        public VehicleType VehicleType { get; set; } = VehicleType.Car;

        [Required(ErrorMessage = "Booking Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "Start Time is required.")]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; } = DateTime.UtcNow.TimeOfDay.Add(TimeSpan.FromMinutes(30 - (DateTime.UtcNow.Minute % 30)));

        [Required(ErrorMessage = "Duration in hours is required.")]
        [Range(1, 48, ErrorMessage = "Duration must be between 1 and 48 hours.")]
        [Display(Name = "Duration (Hours)")]
        public int DurationHours { get; set; } = 2;

        public decimal PricePerHour { get; set; }
        public decimal TotalAmount { get; set; }

        public List<SlotDetailDto> AvailableSlots { get; set; } = new List<SlotDetailDto>();
    }

    public class BookingSummaryViewModel
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string ParkingName { get; set; } = string.Empty;
        public string ParkingAddress { get; set; } = string.Empty;
        public string ParkingCity { get; set; } = string.Empty;
        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookingDetailsViewModel
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserPhone { get; set; }

        public int ParkingSpaceId { get; set; }
        public string ParkingSpaceName { get; set; } = string.Empty;
        public string ParkingSpaceAddress { get; set; } = string.Empty;
        public string ParkingSpaceCity { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public int ParkingSlotId { get; set; }
        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = string.Empty;

        public string VehiclePlateNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // Payment Info
        public int? PaymentId { get; set; }
        public string? TransactionId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }

        // Refund Info
        public int? RefundId { get; set; }
        public decimal? RefundAmount { get; set; }
        public RefundStatus? RefundStatus { get; set; }
        public string? RefundReason { get; set; }

        // QR Code
        public string? QrCodeBase64 { get; set; }

        public bool CanBeCancelled => (Status == BookingStatus.Requested || Status == BookingStatus.Confirmed) && StartTime > DateTime.UtcNow;
        public bool IsActiveNow => Status == BookingStatus.Active || (Status == BookingStatus.Confirmed && StartTime <= DateTime.UtcNow && EndTime >= DateTime.UtcNow);
        public bool IsOverstay => (Status == BookingStatus.Active || Status == BookingStatus.Confirmed) && DateTime.UtcNow > EndTime;
    }

    public class BookingHistoryViewModel
    {
        public string Filter { get; set; } = "all"; // all, upcoming, active, completed, cancelled
        public List<BookingSummaryViewModel> Bookings { get; set; } = new List<BookingSummaryViewModel>();
        public int TotalCount => Bookings.Count;
        public int UpcomingCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class DigitalPassViewModel
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string ParkingName { get; set; } = string.Empty;
        public string ParkingAddress { get; set; } = string.Empty;
        public string ParkingCity { get; set; } = string.Empty;
        public string SlotNumber { get; set; } = string.Empty;
        public string FloorOrZone { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public string QrCodeBase64 { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }

    public class CancelBookingRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Please provide a reason for cancellation.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
