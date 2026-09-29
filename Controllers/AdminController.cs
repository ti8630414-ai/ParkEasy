using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkEasy.Web.Data;
using ParkEasy.Web.Models.Entities;
using ParkEasy.Web.Models.Enums;
using ParkEasy.Web.Models.ViewModels;
using ParkEasy.Web.Services.Interfaces;

namespace ParkEasy.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IParkingService _parkingService;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IParkingService parkingService,
            IBookingService bookingService,
            IPaymentService paymentService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _parkingService = parkingService;
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalSpaces = await _context.ParkingSpaces.CountAsync();
            var totalSlots = await _context.ParkingSlots.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var activeBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Active);
            var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Paid).SumAsync(p => p.Amount);
            var totalRefunded = await _context.Refunds.Where(r => r.Status == RefundStatus.Processed).SumAsync(r => r.Amount);
            var pendingRefunds = await _context.Refunds.CountAsync(r => r.Status == RefundStatus.Pending);

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var userDtos = new List<ApplicationUserDto>();
            foreach (var u in recentUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userDtos.Add(new ApplicationUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "N/A",
                    PhoneNumber = u.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "Driver",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });
            }

            var allSpaces = await _parkingService.GetAllSpacesAsync();
            var allBookings = await _bookingService.GetAllBookingsAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalParkingSpaces = totalSpaces,
                TotalSlots = totalSlots,
                TotalBookings = totalBookings,
                TotalActiveBookings = activeBookings,
                TotalSystemRevenue = totalRevenue,
                TotalRefundedAmount = totalRefunded,
                PendingRefundsCount = pendingRefunds,
                RecentUsers = userDtos,
                RecentBookings = allBookings.Take(6).ToList(),
                AllSpaces = allSpaces
            };

            return View(model);
        }

        public async Task<IActionResult> Users(string? searchQuery, string? selectedRole)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) || (u.Email != null && u.Email.ToLower().Contains(term)));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var dtos = new List<ApplicationUserDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var roleName = roles.FirstOrDefault() ?? "Driver";

                if (!string.IsNullOrWhiteSpace(selectedRole) && selectedRole != "All" && roleName != selectedRole)
                    continue;

                var bookingsCount = await _context.Bookings.CountAsync(b => b.UserId == u.Id);
                var spacesCount = await _context.ParkingSpaces.CountAsync(s => s.OwnerId == u.Id);

                dtos.Add(new ApplicationUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "N/A",
                    PhoneNumber = u.PhoneNumber,
                    VehiclePlateNumber = u.VehiclePlateNumber,
                    Role = roleName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    TotalBookings = bookingsCount,
                    TotalSpacesOwned = spacesCount
                });
            }

            var model = new UserManagementViewModel
            {
                Users = dtos,
                SearchQuery = searchQuery,
                SelectedRole = selectedRole
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User '{user.FullName}' status changed to {(user.IsActive ? "Active" : "Deactivated")}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(EditUserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!await _roleManager.RoleExistsAsync(model.NewRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.NewRole));
            }

            await _userManager.AddToRoleAsync(user, model.NewRole);

            TempData["SuccessMessage"] = $"Role for '{user.FullName}' updated to {model.NewRole}.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Spaces()
        {
            var spaces = await _parkingService.GetAllSpacesAsync();
            return View(spaces);
        }

        public async Task<IActionResult> Bookings(string? status)
        {
            var bookings = await _bookingService.GetAllBookingsAsync(status);
            ViewBag.SelectedStatus = status;
            return View(bookings);
        }

        public async Task<IActionResult> Payments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        public async Task<IActionResult> Refunds()
        {
            var refunds = await _paymentService.GetAllRefundsAsync();
            return View(refunds);
        }

        public async Task<IActionResult> SystemHealth()
        {
            var model = new SystemHealthViewModel
            {
                TotalRegisteredUsers = await _userManager.Users.CountAsync(),
                TotalSpaces = await _context.ParkingSpaces.CountAsync(),
                TotalSlots = await _context.ParkingSlots.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalPayments = await _context.Payments.CountAsync(),
                TotalNotifications = await _context.Notifications.CountAsync(),
                ServerTimeUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            return View(model);
        }
    }
}
