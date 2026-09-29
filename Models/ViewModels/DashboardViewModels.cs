using System;
using System.Collections.Generic;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;

namespace ParkEasy.Web.Models.ViewModels
{
    public class OwnerDashboardViewModel
    {
        public int TotalParkingSpaces { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int TotalBookings { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }

        public decimal TotalEarnings { get; set; }
        public decimal TodayEarnings { get; set; }
        public double OccupancyRatePercent { get; set; }

        public List<ParkingSpaceCardViewModel> Spaces { get; set; } = new List<ParkingSpaceCardViewModel>();
        public List<BookingSummaryViewModel> RecentBookings { get; set; } = new List<BookingSummaryViewModel>();
        public List<BookingSummaryViewModel> PendingBookingsList { get; set; } = new List<BookingSummaryViewModel>();
        public List<MonthlyEarningDto> MonthlyEarnings { get; set; } = new List<MonthlyEarningDto>();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalOwners { get; set; }
        public int TotalParkingSpaces { get; set; }
        public int TotalSlots { get; set; }
        public int TotalBookings { get; set; }
        public int TotalActiveBookings { get; set; }
        public decimal TotalSystemRevenue { get; set; }
        public decimal TotalRefundedAmount { get; set; }
        public int PendingRefundsCount { get; set; }

        public List<ApplicationUserDto> RecentUsers { get; set; } = new List<ApplicationUserDto>();
        public List<BookingSummaryViewModel> RecentBookings { get; set; } = new List<BookingSummaryViewModel>();
        public List<ParkingSpaceCardViewModel> AllSpaces { get; set; } = new List<ParkingSpaceCardViewModel>();
    }

    public class MonthlyEarningDto
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int BookingsCount { get; set; }
    }

    public class OccupancyStatsDto
    {
        public string SpaceName { get; set; } = string.Empty;
        public int TotalSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class ApplicationUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalBookings { get; set; }
        public int TotalSpacesOwned { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<ApplicationUserDto> Users { get; set; } = new List<ApplicationUserDto>();
        public string? SearchQuery { get; set; }
        public string? SelectedRole { get; set; }
    }

    public class EditUserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SystemHealthViewModel
    {
        public string DatabaseStatus { get; set; } = "Connected (PostgreSQL / Npgsql)";
        public string SignalRStatus { get; set; } = "Active (/parkingHub)";
        public string QrGeneratorStatus { get; set; } = "Operational (QRCoder Base64 Engine)";
        public string ServerTimeUtc { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        public string MemoryUsage { get; set; } = "Normal";
        public int TotalRegisteredUsers { get; set; }
        public int TotalSpaces { get; set; }
        public int TotalSlots { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPayments { get; set; }
        public int TotalNotifications { get; set; }
    }
}
