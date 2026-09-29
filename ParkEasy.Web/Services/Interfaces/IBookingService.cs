using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;

namespace ParkEasy.Web.Services.Interfaces
{
    public interface IBookingService
    {
        Task<bool> IsSlotAvailableAsync(int slotId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
        Task<(bool Success, string Message, Booking? Booking)> CreateBookingAsync(CreateBookingViewModel model, string userId);
        Task<BookingDetailsViewModel?> GetBookingDetailsAsync(int id, string? currentUserId = null, bool isElevated = false);
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<BookingHistoryViewModel> GetUserBookingsAsync(string userId, string filter = "all");
        Task<List<BookingSummaryViewModel>> GetBookingsByOwnerIdAsync(string ownerId, string? status = null);
        Task<List<BookingSummaryViewModel>> GetAllBookingsAsync(string? status = null);
        Task<DigitalPassViewModel?> GetDigitalPassAsync(int bookingId, string currentUserId, bool isElevated = false);

        // Actions
        Task<bool> ApproveBookingAsync(int bookingId, string currentUserId, bool isAdmin = false);
        Task<bool> RejectBookingAsync(int bookingId, string reason, string currentUserId, bool isAdmin = false);
        Task<bool> CheckInBookingAsync(int bookingId, string currentUserId, bool isElevated = false);
        Task<bool> CompleteBookingAsync(int bookingId, string currentUserId, bool isElevated = false);
        Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string reason, string currentUserId, bool isAdmin = false);
        Task<int> ProcessExpiredAndCompletedBookingsAsync();
    }
}
