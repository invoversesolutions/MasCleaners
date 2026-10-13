using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MasCleaners.Controllers
{
    public class BookingController : Controller
    {
        // =========================================================
        // DEPENDENCIES
        // =========================================================

        private readonly ILogger<BookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private const string CartCookieName = "MAS_CART";

        public BookingController(
            ILogger<BookingController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        // =========================================================
        // CREATE BOOKING - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation(
                    "Loading booking creation page.");

                // =================================================
                // IDENTIFY THE VISITOR
                // =================================================

                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                var isAuthenticated =
                    User.Identity?.IsAuthenticated == true &&
                    !string.IsNullOrWhiteSpace(userId);


                // =================================================
                // LOAD THE CORRECT CART
                // =================================================

                Cart? cart;

                if (isAuthenticated)
                {
                    cart = await _unitOfWork.Cart.GetAsync(
                        x => x.UserId == userId && x.IsActive,
                        includeProperties:
                            "CartItems,CartItems.ServiceOption");
                }
                else
                {
                    var guestToken =
                        Request.Cookies[CartCookieName];

                    if (string.IsNullOrWhiteSpace(guestToken))
                    {
                        TempData["ErrorMessage"] =
                            "Your cart is empty. Please select a service first.";

                        return RedirectToAction("Cart", "Cart");
                    }

                    cart = await _unitOfWork.Cart.GetAsync(
                        x => x.GuestToken == guestToken
                             && x.UserId == null
                             && x.IsActive,
                        includeProperties:
                            "CartItems,CartItems.ServiceOption");
                }


                // =================================================
                // VALIDATE CART
                // =================================================

                if (cart == null ||
                    cart.CartItems == null ||
                    !cart.CartItems.Any())
                {
                    TempData["ErrorMessage"] =
                        "Please add a cleaning service before booking.";

                    return RedirectToAction("Cart", "Cart");
                }


                // =================================================
                // INITIALISE VIEW MODEL
                // =================================================

                var model = new BookingCreateVM
                {
                    IsAuthenticated = isAuthenticated,

                    BookingDate = DateTime.Today.AddDays(1),

                    BookingTime = new TimeSpan(7, 0, 0)
                };


                // =================================================
                // POPULATE CUSTOMER DETAILS
                // =================================================

                if (isAuthenticated)
                {
                    var customer =
                        await _unitOfWork.Customer.GetAsync(
                            x => x.ApplicationUserId == userId,
                            includeProperties:
                                "ApplicationUser,Addresses");

                    if (customer != null)
                    {
                        model.FirstName = customer.FirstName;
                        model.LastName = customer.LastName;
                        model.PhoneNumber = customer.PhoneNumber;
                        model.CustomerType = customer.CustomerType;
                        model.CompanyName = customer.CompanyName;
                        model.VatNumber = customer.VatNumber;

                        model.Email =
                            customer.ApplicationUser?.Email
                            ?? User.FindFirstValue(ClaimTypes.Email)
                            ?? string.Empty;

                        model.AvailableAddresses =
                            customer.Addresses?
                                .Where(x => x.IsActive)
                                .ToList()
                            ?? new List<CustomerAddress>();
                    }
                    else
                    {
                        // An authenticated account without a Customer
                        // record is still an authenticated account.
                        model.Email =
                            User.FindFirstValue(ClaimTypes.Email)
                            ?? string.Empty;

                        _logger.LogWarning(
                            "No Customer record exists for authenticated user {UserId}.",
                            userId);
                    }
                }


                // =================================================
                // POPULATE CART SUMMARY
                // =================================================

                foreach (var item in cart.CartItems)
                {
                    if (item.ServiceOption == null)
                    {
                        continue;
                    }

                    var unitPrice = item.ServiceOption.Price;

                    model.CartItems.Add(new BookingCartItemVM
                    {
                        ServiceOptionId = item.ServiceOptionId,

                        ServiceName = item.ServiceOption.Name,

                        Quantity = item.Quantity,

                        UnitPrice = unitPrice,

                        TotalPrice = unitPrice * item.Quantity
                    });
                }

                model.Subtotal = model.CartItems.Sum(
                    x => x.TotalPrice);

                model.Total = model.Subtotal;


                // =================================================
                // RETURN BOOKING VIEW
                // =================================================

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading booking creation page.");

                TempData["ErrorMessage"] =
                    "Unable to load the booking page. Please try again.";

                return RedirectToAction("Cart", "Cart");
            }
        }
    }
}