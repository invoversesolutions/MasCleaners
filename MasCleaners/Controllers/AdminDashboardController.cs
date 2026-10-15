using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels.AdminDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            IUnitOfWork unitOfWork,
            ILogger<AdminDashboardController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation(
                    "Loading the administrator bookings dashboard.");

                var bookings = (await _unitOfWork.Booking
                    .GetAllAsync(
                        includeProperties: "Customer,BookingItems"))
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();

                var model = new AdminDashboardVM
                {
                    TotalBookings = bookings.Count,

                    PendingBookings = bookings.Count(b =>
                        string.Equals(
                            b.Status.ToString(),
                            BookingStatus.Pending.ToString(),
                            StringComparison.OrdinalIgnoreCase)),

                    ConfirmedBookings = bookings.Count(b =>
                        string.Equals(
                            b.Status.ToString(),
                            BookingStatus.Confirmed.ToString(),
                            StringComparison.OrdinalIgnoreCase)),

                    CompletedBookings = bookings.Count(b =>
                        string.Equals(
                            b.Status.ToString(),
                            BookingStatus.Completed.ToString(),
                            StringComparison.OrdinalIgnoreCase)),

                    RecentBookings = bookings
                        .Take(10)
                        .ToList()
                };

                _logger.LogInformation(
                    "Bookings dashboard loaded. Total: {Total}, Pending: {Pending}, Confirmed: {Confirmed}, Completed: {Completed}.",
                    model.TotalBookings,
                    model.PendingBookings,
                    model.ConfirmedBookings,
                    model.CompletedBookings);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load the administrator bookings dashboard.");

                TempData["ErrorMessage"] =
                    "Unable to load the bookings overview.";

                return View(new AdminDashboardVM());
            }
        }
    }
}