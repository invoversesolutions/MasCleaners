
using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IUnitOfWork unitOfWork,
            ILogger<CustomerController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading customer management page.");

            try
            {
                var customers = await _unitOfWork.Customer.GetAllAsync(
                    includeProperties: "ApplicationUser"
                );

                var customerList = customers
                    .OrderBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .ToList();

                _logger.LogInformation(
                    "Successfully loaded {CustomerCount} customers.",
                    customerList.Count
                );

                return View(customerList);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while loading customers."
                );

                TempData["ErrorMessage"] =
                    "Unable to load customers at this time. Please try again.";

                return View(new List<Customer>());
            }
        }
    }
}
