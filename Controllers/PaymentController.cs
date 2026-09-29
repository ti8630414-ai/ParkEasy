using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> Simulate(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isElevated = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Owner);

            var booking = await _bookingService.GetBookingDetailsAsync(bookingId, userId, isElevated);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found or unauthorized.";
                return RedirectToAction("History", "Booking");
            }

            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["InfoMessage"] = "This booking has already been paid.";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            var model = new PaymentSimulatorViewModel
            {
                BookingId = booking.Id,
                BookingReference = booking.BookingReference,
                ParkingName = booking.ParkingSpaceName,
                SlotNumber = booking.SlotNumber,
                UserName = booking.UserName,
                UserEmail = booking.UserEmail,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                DurationHours = booking.DurationHours,
                Amount = booking.TotalAmount,
                PaymentMethod = PaymentMethod.Bkash,
                SimulateSuccess = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulate(PaymentSimulatorViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var result = await _paymentService.ProcessPaymentSimulationAsync(model, userId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return View("Result", result);
        }
    }
}
