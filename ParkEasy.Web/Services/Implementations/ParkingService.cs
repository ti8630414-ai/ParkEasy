using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Data;
using ParkEasy.Web.Helpers;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Services.Implementations
{
    public class ParkingService : IParkingService
    {
        private readonly ApplicationDbContext _context;

        public ParkingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingSearchViewModel> SearchParkingSpacesAsync(ParkingSearchViewModel model)
        {
            var query = _context.ParkingSpaces
                .Include(p => p.Slots)
                .Include(p => p.Bookings)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Text search (Name, Address, City)
            if (!string.IsNullOrWhiteSpace(model.Query))
            {
                var term = model.Query.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                         p.Address.ToLower().Contains(term) ||
                                         p.City.ToLower().Contains(term) ||
                                         (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            // City filter
            if (!string.IsNullOrWhiteSpace(model.City) && model.City != "All")
            {
                query = query.Where(p => p.City.ToLower() == model.City.Trim().ToLower());
            }

            // Price filter
            if (model.MaxPrice.HasValue && model.MaxPrice > 0)
            {
                query = query.Where(p => p.BasePricePerHour <= model.MaxPrice.Value);
            }

            // Amenities filters
            if (model.HasCCTV) query = query.Where(p => p.HasCCTV);
            if (model.HasEVCharging) query = query.Where(p => p.HasEVCharging);
            if (model.HasCoveredParking) query = query.Where(p => p.HasCoveredParking);
            if (model.HasDisabledAccess) query = query.Where(p => p.HasDisabledAccess);
            if (model.HasValet) query = query.Where(p => p.HasValet);

            var spaces = await query.ToListAsync();

            var now = DateTime.UtcNow;

            var resultCards = new List<ParkingSpaceCardViewModel>();

            foreach (var space in spaces)
            {
                // Filter slots by vehicle type if requested
                var relevantSlots = space.Slots.Where(s => s.IsActive).ToList();
                if (model.VehicleType.HasValue)
                {
                    relevantSlots = relevantSlots.Where(s => s.SupportedVehicleType == model.VehicleType.Value).ToList();
                    if (!relevantSlots.Any())
                        continue; // Skip spaces with no slots for this vehicle type
                }

                // Calculate currently available slots
                var activeBookings = space.Bookings
                    .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                                b.StartTime <= now && b.EndTime >= now)
                    .Select(b => b.ParkingSlotId)
                    .ToHashSet();

                int totalSlots = relevantSlots.Count;
                int availableSlots = relevantSlots.Count(s => !activeBookings.Contains(s.Id) && s.CurrentStatus != SlotStatus.Maintenance);

                double? distance = null;
                if (model.Latitude.HasValue && model.Longitude.HasValue)
                {
                    distance = CalculateDistance(
                        model.Latitude.Value,
                        model.Longitude.Value,
                        (double)space.Latitude,
                        (double)space.Longitude);

                    // Radius filter
                    if (model.RadiusKm > 0 && distance > model.RadiusKm)
                        continue;
                }

                resultCards.Add(new ParkingSpaceCardViewModel
                {
                    Id = space.Id,
                    Name = space.Name,
                    Description = space.Description ?? string.Empty,
                    Address = space.Address,
                    City = space.City,
                    Latitude = space.Latitude,
                    Longitude = space.Longitude,
                    BasePricePerHour = space.BasePricePerHour,
                    ImageUrl = space.ImageUrl,
                    TotalSlots = totalSlots,
                    AvailableSlots = availableSlots,
                    DistanceKm = distance.HasValue ? Math.Round(distance.Value, 2) : null,
                    HasCCTV = space.HasCCTV,
                    HasEVCharging = space.HasEVCharging,
                    HasCoveredParking = space.HasCoveredParking,
                    HasDisabledAccess = space.HasDisabledAccess,
                    HasValet = space.HasValet,
                    HasSecurityGuard = space.HasSecurityGuard,
                    Is24Hours = space.Is24Hours,
                    OperatingHours = space.Is24Hours ? "24/7" : $"{space.OpeningTime:hh\\:mm} - {space.ClosingTime:hh\\:mm}"
                });
            }

            // Sorting
            resultCards = model.SortBy switch
            {
                "price_desc" => resultCards.OrderByDescending(c => c.BasePricePerHour).ToList(),
                "price_asc" => resultCards.OrderBy(c => c.BasePricePerHour).ToList(),
                "distance" when model.Latitude.HasValue => resultCards.OrderBy(c => c.DistanceKm ?? double.MaxValue).ToList(),
                "availability" => resultCards.OrderByDescending(c => c.AvailableSlots).ToList(),
                _ => resultCards.OrderBy(c => c.BasePricePerHour).ToList()
            };

            model.Results = resultCards;
            model.AvailableCities = await GetDistinctCitiesAsync();

            return model;
        }

        public async Task<ParkingSpaceDetailsViewModel?> GetParkingSpaceDetailsAsync(int id, DateTime? targetDate = null, TimeSpan? startTime = null, int durationHours = 2)
        {
            var space = await _context.ParkingSpaces
                .Include(p => p.Owner)
                .Include(p => p.Slots)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (space == null)
                return null;

            var date = targetDate.HasValue ? DateTimeHelper.ToUtc(targetDate.Value) : DateTime.UtcNow.Date;
            var time = startTime ?? DateTime.UtcNow.TimeOfDay;
            var requestedStart = DateTimeHelper.CombineToUtc(date, time);
            var requestedEnd = requestedStart.AddHours(durationHours > 0 ? durationHours : 2);

            var slotDtos = await GetSlotsForSpaceAsync(id, requestedStart, requestedEnd);

            return new ParkingSpaceDetailsViewModel
            {
                Id = space.Id,
                Name = space.Name,
                Description = space.Description ?? string.Empty,
                Address = space.Address,
                City = space.City,
                State = space.State,
                PostalCode = space.PostalCode,
                Latitude = space.Latitude,
                Longitude = space.Longitude,
                BasePricePerHour = space.BasePricePerHour,
                ImageUrl = space.ImageUrl,
                OwnerName = space.Owner.FullName ?? space.Owner.UserName ?? "Host",
                OwnerPhone = space.Owner.PhoneNumber ?? "N/A",
                OwnerEmail = space.Owner.Email ?? "N/A",
                Is24Hours = space.Is24Hours,
                OpeningTime = space.OpeningTime,
                ClosingTime = space.ClosingTime,
                HasCCTV = space.HasCCTV,
                HasEVCharging = space.HasEVCharging,
                HasCoveredParking = space.HasCoveredParking,
                HasDisabledAccess = space.HasDisabledAccess,
                HasValet = space.HasValet,
                HasSecurityGuard = space.HasSecurityGuard,
                TotalSlotsCount = slotDtos.Count,
                AvailableSlotsCount = slotDtos.Count(s => s.IsAvailableForRequestedTime),
                Slots = slotDtos,
                SelectedDate = date,
                SelectedStartTime = time,
                DurationHours = durationHours
            };
        }

        public async Task<ParkingSpace?> GetParkingSpaceByIdAsync(int id)
        {
            return await _context.ParkingSpaces
                .Include(p => p.Owner)
                .Include(p => p.Slots)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<ParkingSpaceCardViewModel>> GetSpacesByOwnerIdAsync(string ownerId)
        {
            var spaces = await _context.ParkingSpaces
                .Include(p => p.Slots)
                .Include(p => p.Bookings)
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return spaces.Select(s => {
                var activeBookingSlotIds = s.Bookings
                    .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                                b.StartTime <= now && b.EndTime >= now)
                    .Select(b => b.ParkingSlotId)
                    .ToHashSet();

                return new ParkingSpaceCardViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description ?? string.Empty,
                    Address = s.Address,
                    City = s.City,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    BasePricePerHour = s.BasePricePerHour,
                    ImageUrl = s.ImageUrl,
                    TotalSlots = s.Slots.Count,
                    AvailableSlots = s.Slots.Count(slot => !activeBookingSlotIds.Contains(slot.Id) && slot.IsActive && slot.CurrentStatus != SlotStatus.Maintenance),
                    HasCCTV = s.HasCCTV,
                    HasEVCharging = s.HasEVCharging,
                    HasCoveredParking = s.HasCoveredParking,
                    HasDisabledAccess = s.HasDisabledAccess,
                    HasValet = s.HasValet,
                    HasSecurityGuard = s.HasSecurityGuard,
                    Is24Hours = s.Is24Hours,
                    OperatingHours = s.Is24Hours ? "24/7" : $"{s.OpeningTime:hh\\:mm} - {s.ClosingTime:hh\\:mm}"
                };
            }).ToList();
        }

        public async Task<List<ParkingSpaceCardViewModel>> GetAllSpacesAsync()
        {
            var spaces = await _context.ParkingSpaces
                .Include(p => p.Slots)
                .Include(p => p.Bookings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return spaces.Select(s => {
                var activeBookingSlotIds = s.Bookings
                    .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                                b.StartTime <= now && b.EndTime >= now)
                    .Select(b => b.ParkingSlotId)
                    .ToHashSet();

                return new ParkingSpaceCardViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description ?? string.Empty,
                    Address = s.Address,
                    City = s.City,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    BasePricePerHour = s.BasePricePerHour,
                    ImageUrl = s.ImageUrl,
                    TotalSlots = s.Slots.Count,
                    AvailableSlots = s.Slots.Count(slot => !activeBookingSlotIds.Contains(slot.Id) && slot.IsActive && slot.CurrentStatus != SlotStatus.Maintenance),
                    HasCCTV = s.HasCCTV,
                    HasEVCharging = s.HasEVCharging,
                    HasCoveredParking = s.HasCoveredParking,
                    HasDisabledAccess = s.HasDisabledAccess,
                    HasValet = s.HasValet,
                    HasSecurityGuard = s.HasSecurityGuard,
                    Is24Hours = s.Is24Hours,
                    OperatingHours = s.Is24Hours ? "24/7" : $"{s.OpeningTime:hh\\:mm} - {s.ClosingTime:hh\\:mm}"
                };
            }).ToList();
        }

        public async Task<ParkingSpace> CreateParkingSpaceAsync(ParkingSpaceCreateEditViewModel model, string ownerId)
        {
            var space = new ParkingSpace
            {
                OwnerId = ownerId,
                Name = model.Name,
                Description = model.Description,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                BasePricePerHour = model.BasePricePerHour,
                Is24Hours = model.Is24Hours,
                OpeningTime = model.OpeningTime,
                ClosingTime = model.ClosingTime,
                ImageUrl = model.ImageUrl,
                HasCCTV = model.HasCCTV,
                HasEVCharging = model.HasEVCharging,
                HasCoveredParking = model.HasCoveredParking,
                HasDisabledAccess = model.HasDisabledAccess,
                HasValet = model.HasValet,
                HasSecurityGuard = model.HasSecurityGuard,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParkingSpaces.Add(space);
            await _context.SaveChangesAsync();

            // Auto-generate slots if requested
            if (model.AutoGenerateSlotsCount > 0)
            {
                var slots = new List<ParkingSlot>();
                for (int i = 1; i <= model.AutoGenerateSlotsCount; i++)
                {
                    slots.Add(new ParkingSlot
                    {
                        ParkingSpaceId = space.Id,
                        SlotNumber = $"P-{i:D2}",
                        FloorOrZone = string.IsNullOrWhiteSpace(model.DefaultZone) ? "Ground" : model.DefaultZone,
                        SupportedVehicleType = (i % 4 == 0) ? VehicleType.SUV : ((i % 5 == 0) ? VehicleType.ElectricVehicle : VehicleType.Car),
                        PricePerHour = model.BasePricePerHour,
                        HasEVCharger = (i % 5 == 0) || model.HasEVCharging,
                        IsActive = true,
                        CurrentStatus = SlotStatus.Available,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                _context.ParkingSlots.AddRange(slots);
                await _context.SaveChangesAsync();
            }

            return space;
        }

        public async Task<bool> UpdateParkingSpaceAsync(ParkingSpaceCreateEditViewModel model, string currentUserId, bool isAdmin = false)
        {
            var space = await _context.ParkingSpaces.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (space == null) return false;

            if (!isAdmin && space.OwnerId != currentUserId)
                return false;

            space.Name = model.Name;
            space.Description = model.Description;
            space.Address = model.Address;
            space.City = model.City;
            space.State = model.State;
            space.PostalCode = model.PostalCode;
            space.Latitude = model.Latitude;
            space.Longitude = model.Longitude;
            space.BasePricePerHour = model.BasePricePerHour;
            space.Is24Hours = model.Is24Hours;
            space.OpeningTime = model.OpeningTime;
            space.ClosingTime = model.ClosingTime;
            space.ImageUrl = model.ImageUrl;
            space.HasCCTV = model.HasCCTV;
            space.HasEVCharging = model.HasEVCharging;
            space.HasCoveredParking = model.HasCoveredParking;
            space.HasDisabledAccess = model.HasDisabledAccess;
            space.HasValet = model.HasValet;
            space.HasSecurityGuard = model.HasSecurityGuard;
            space.IsActive = model.IsActive;
            space.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteParkingSpaceAsync(int id, string currentUserId, bool isAdmin = false)
        {
            var space = await _context.ParkingSpaces.FirstOrDefaultAsync(p => p.Id == id);
            if (space == null) return false;

            if (!isAdmin && space.OwnerId != currentUserId)
                return false;

            _context.ParkingSpaces.Remove(space);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SlotDetailDto>> GetSlotsForSpaceAsync(int spaceId, DateTime? startTime = null, DateTime? endTime = null)
        {
            var slots = await _context.ParkingSlots
                .Where(s => s.ParkingSpaceId == spaceId && s.IsActive)
                .OrderBy(s => s.FloorOrZone)
                .ThenBy(s => s.SlotNumber)
                .ToListAsync();

            var start = startTime.HasValue ? DateTimeHelper.ToUtc(startTime.Value) : DateTime.UtcNow;
            var end = endTime.HasValue ? DateTimeHelper.ToUtc(endTime.Value) : start.AddHours(2);

            // Find overlapping booked slots
            var bookedSlotIds = await _context.Bookings
                .Where(b => b.ParkingSpaceId == spaceId &&
                            (b.Status == BookingStatus.Requested || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) &&
                            b.StartTime < end && b.EndTime > start)
                .Select(b => b.ParkingSlotId)
                .Distinct()
                .ToListAsync();

            var bookedSlotSet = bookedSlotIds.ToHashSet();

            return slots.Select(s => new SlotDetailDto
            {
                Id = s.Id,
                SlotNumber = s.SlotNumber,
                FloorOrZone = s.FloorOrZone,
                SupportedVehicleType = s.SupportedVehicleType,
                PricePerHour = s.PricePerHour,
                IsActive = s.IsActive,
                CurrentStatus = bookedSlotSet.Contains(s.Id) ? SlotStatus.Occupied : s.CurrentStatus,
                HasEVCharger = s.HasEVCharger,
                IsAvailableForRequestedTime = !bookedSlotSet.Contains(s.Id) && s.CurrentStatus != SlotStatus.Maintenance
            }).ToList();
        }

        public async Task<ParkingSlot?> GetSlotByIdAsync(int slotId)
        {
            return await _context.ParkingSlots
                .Include(s => s.ParkingSpace)
                .FirstOrDefaultAsync(s => s.Id == slotId);
        }

        public async Task<ParkingSlot> AddSlotAsync(SlotCreateEditViewModel model)
        {
            var slot = new ParkingSlot
            {
                ParkingSpaceId = model.ParkingSpaceId,
                SlotNumber = model.SlotNumber.Trim().ToUpper(),
                FloorOrZone = model.FloorOrZone.Trim(),
                SupportedVehicleType = model.SupportedVehicleType,
                PricePerHour = model.PricePerHour,
                HasEVCharger = model.HasEVCharger,
                IsActive = model.IsActive,
                CurrentStatus = model.CurrentStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParkingSlots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task<bool> UpdateSlotAsync(SlotCreateEditViewModel model)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (slot == null) return false;

            slot.SlotNumber = model.SlotNumber.Trim().ToUpper();
            slot.FloorOrZone = model.FloorOrZone.Trim();
            slot.SupportedVehicleType = model.SupportedVehicleType;
            slot.PricePerHour = model.PricePerHour;
            slot.HasEVCharger = model.HasEVCharger;
            slot.IsActive = model.IsActive;
            slot.CurrentStatus = model.CurrentStatus;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSlotAsync(int slotId)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null) return false;

            _context.ParkingSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSlotStatusAsync(int slotId, bool isActive)
        {
            var slot = await _context.ParkingSlots.FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null) return false;

            slot.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetDistinctCitiesAsync()
        {
            return await _context.ParkingSpaces
                .Where(p => p.IsActive)
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
    }
}
