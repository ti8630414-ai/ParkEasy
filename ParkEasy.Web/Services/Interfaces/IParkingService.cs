using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.ViewModels;

namespace ParkEasy.Web.Services.Interfaces
{
    public interface IParkingService
    {
        Task<ParkingSearchViewModel> SearchParkingSpacesAsync(ParkingSearchViewModel model);
        Task<ParkingSpaceDetailsViewModel?> GetParkingSpaceDetailsAsync(int id, DateTime? targetDate = null, TimeSpan? startTime = null, int durationHours = 2);
        Task<ParkingSpace?> GetParkingSpaceByIdAsync(int id);
        Task<List<ParkingSpaceCardViewModel>> GetSpacesByOwnerIdAsync(string ownerId);
        Task<List<ParkingSpaceCardViewModel>> GetAllSpacesAsync();
        Task<ParkingSpace> CreateParkingSpaceAsync(ParkingSpaceCreateEditViewModel model, string ownerId);
        Task<bool> UpdateParkingSpaceAsync(ParkingSpaceCreateEditViewModel model, string currentUserId, bool isAdmin = false);
        Task<bool> DeleteParkingSpaceAsync(int id, string currentUserId, bool isAdmin = false);

        // Slots
        Task<List<SlotDetailDto>> GetSlotsForSpaceAsync(int spaceId, DateTime? startTime = null, DateTime? endTime = null);
        Task<ParkingSlot?> GetSlotByIdAsync(int slotId);
        Task<ParkingSlot> AddSlotAsync(SlotCreateEditViewModel model);
        Task<bool> UpdateSlotAsync(SlotCreateEditViewModel model);
        Task<bool> DeleteSlotAsync(int slotId);
        Task<bool> ToggleSlotStatusAsync(int slotId, bool isActive);

        // Utilities
        Task<List<string>> GetDistinctCitiesAsync();
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}
