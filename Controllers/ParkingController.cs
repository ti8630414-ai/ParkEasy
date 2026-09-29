using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ParkEasy.Web.Helpers;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    public class ParkingController : Controller
    {
        private readonly IParkingService _parkingService;
        private readonly IConfiguration _configuration;

        public ParkingController(IParkingService parkingService, IConfiguration configuration)
        {
            _parkingService = parkingService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ParkingSearchViewModel model)
        {
            var results = await _parkingService.SearchParkingSpacesAsync(model);
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? "YOUR_GOOGLE_MAPS_API_KEY";
            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, [FromQuery] DateTime? targetDate, [FromQuery] TimeSpan? startTime, [FromQuery] int durationHours = 2)
        {
            var details = await _parkingService.GetParkingSpaceDetailsAsync(id, targetDate, startTime, durationHours);
            if (details == null)
            {
                TempData["ErrorMessage"] = "The requested parking space was not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? "YOUR_GOOGLE_MAPS_API_KEY";
            return View(details);
        }

        [HttpGet]
        public async Task<IActionResult> GetSlotAvailability(int spaceId, DateTime date, TimeSpan startTime, int durationHours)
        {
            var startDateTime = DateTimeHelper.CombineToUtc(date, startTime);
            var endDateTime = startDateTime.AddHours(durationHours > 0 ? durationHours : 1);

            var slots = await _parkingService.GetSlotsForSpaceAsync(spaceId, startDateTime, endDateTime);
            return Json(new { success = true, slots });
        }
    }
}
