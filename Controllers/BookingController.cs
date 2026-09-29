using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParkEasy.Web.Helpers;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IParkingService _parkingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(
            IBookingService bookingService,
            IParkingService parkingService,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _parkingService = parkingService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int spaceId, int? slotId, DateTime? date, TimeSpan? startTime, int durationHours = 2)
        {
            var space = await _parkingService.GetParkingSpaceByIdAsync(spaceId);
            if (space == null)
            {
                TempData["ErrorMessage"] = "Invalid parking space.";
                return RedirectToAction("Index", "Parking");
            }

            var user = await _userManager.GetUserAsync(User);

            var bookingDate = date.HasValue ? DateTimeHelper.ToUtc(date.Value) : DateTime.UtcNow.Date;
            var bookingStartTime = startTime ?? DateTime.UtcNow.TimeOfDay.Add(TimeSpan.FromMinutes(30 - (DateTime.UtcNow.Minute % 30)));
            var startDateTime = DateTimeHelper.CombineToUtc(bookingDate, bookingStartTime);
            var endDateTime = startDateTime.AddHours(durationHours > 0 ? durationHours : 2);

            var availableSlots = await _parkingService.GetSlotsForSpaceAsync(spaceId, startDateTime, endDateTime);

            var selectedSlot = slotId.HasValue ? availableSlots.Find(s => s.Id == slotId.Value) : availableSlots.Find(s => s.IsAvailableForRequestedTime);

            var model = new CreateBookingViewModel
            {
                ParkingSpaceId = space.Id,
                ParkingSpaceName = space.Name,
                ParkingSpaceAddress = space.Address,
                ParkingSpaceCity = space.City,
                ParkingSlotId = selectedSlot?.Id ?? (availableSlots.Count > 0 ? availableSlots[0].Id : 0),
                SlotNumber = selectedSlot?.SlotNumber ?? string.Empty,
                FloorOrZone = selectedSlot?.FloorOrZone ?? string.Empty,
                VehiclePlateNumber = user?.VehiclePlateNumber ?? string.Empty,
                VehicleType = user?.PreferredVehicleType ?? VehicleType.Car,
                BookingDate = bookingDate,
                StartTime = bookingStartTime,
                DurationHours = durationHours,
                PricePerHour = selectedSlot?.PricePerHour ?? space.BasePricePerHour,
                TotalAmount = (selectedSlot?.PricePerHour ?? space.BasePricePerHour) * durationHours,
                AvailableSlots = availableSlots
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var startDateTime = DateTimeHelper.CombineToUtc(model.BookingDate, model.StartTime);
                var endDateTime = startDateTime.AddHours(model.DurationHours);
                model.AvailableSlots = await _parkingService.GetSlotsForSpaceAsync(model.ParkingSpaceId, startDateTime, endDateTime);
                return View(model);
            }

            var result = await _bookingService.CreateBookingAsync(model, user.Id);
            if (!result.Success || result.Booking == null)
            {
                TempData["ErrorMessage"] = result.Message;
                var startDateTime = DateTimeHelper.CombineToUtc(model.BookingDate, model.StartTime);
                var endDateTime = startDateTime.AddHours(model.DurationHours);
                model.AvailableSlots = await _parkingService.GetSlotsForSpaceAsync(model.ParkingSpaceId, startDateTime, endDateTime);
                return View(model);
            }

            TempData["SuccessMessage"] = "Slot reserved successfully! Please complete payment to finalize your booking.";
            return RedirectToAction(nameof(Confirmation), new { id = result.Booking.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var booking = await _bookingService.GetBookingDetailsAsync(id, userId, isElevated);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or unauthorized.";
                return RedirectToAction(nameof(History));
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> History([FromQuery] string filter = "all")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var history = await _bookingService.GetUserBookingsAsync(userId, filter);
            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var booking = await _bookingService.GetBookingDetailsAsync(id, userId, isElevated);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or access denied.";
                return RedirectToAction(nameof(History));
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> Pass(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var pass = await _bookingService.GetDigitalPassAsync(id, userId ?? string.Empty, isElevated);
            if (pass == null)
            {
                TempData["ErrorMessage"] = "Digital pass not found or access denied.";
                return RedirectToAction(nameof(History));
            }

            return View(pass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var result = await _bookingService.CancelBookingAsync(bookingId, reason, userId ?? string.Empty, isAdmin);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var success = await _bookingService.CheckInBookingAsync(bookingId, userId ?? string.Empty, isElevated);
            if (success)
            {
                TempData["SuccessMessage"] = "Checked in successfully! Slot is now active.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not check in.";
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var success = await _bookingService.CompleteBookingAsync(bookingId, userId ?? string.Empty, isElevated);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking marked as completed. Thank you for using ParkEasy!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not complete booking.";
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}
