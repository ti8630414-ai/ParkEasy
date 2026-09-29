using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IParkingService _parkingService;

        public HomeController(IParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        public async Task<IActionResult> Index()
        {
            var searchModel = new ParkingSearchViewModel();
            var searchResults = await _parkingService.SearchParkingSpacesAsync(searchModel);
            ViewBag.FeaturedSpaces = searchResults.Results.Take(6).ToList();
            ViewBag.Cities = searchResults.AvailableCities;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string message)
        {
            TempData["SuccessMessage"] = "Thank you for contacting ParkEasy support. We will get back to you shortly.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
