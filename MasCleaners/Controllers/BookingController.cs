using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        // =========================================================
        // CREATE BOOKING - POST (AUTHENTICATED CUSTOMERS)
        // =========================================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateVM model)
        {
            try
            {
                _logger.LogInformation(
                    "Processing booking creation request.");

                // =================================================
                // IDENTIFY THE LOGGED-IN USER
                // =================================================

                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Challenge();
                }

                // =================================================
                // LOAD THE CUSTOMER PROFILE
                // =================================================

                var customer =
                    await _unitOfWork.Customer.GetAsync(
                        x => x.ApplicationUserId == userId
                             && x.IsActive,
                        includeProperties: "Addresses");

                if (customer == null)
                {
                    _logger.LogWarning(
                        "Booking rejected because Customer profile " +
                        "was not found for user {UserId}.",
                        userId);

                    TempData["ErrorMessage"] =
                        "Your customer profile could not be found. " +
                        "Please contact support.";

                    return RedirectToAction("Index", "Cart");
                }

                // =================================================
                // LOAD THE AUTHENTICATED USER'S ACTIVE CART
                // =================================================

                var cart = await _unitOfWork.Cart.GetAsync(
                    x => x.UserId == userId && x.IsActive,
                    includeProperties:
                        "CartItems,CartItems.ServiceOption");

                if (cart == null ||
                    cart.CartItems == null ||
                    !cart.CartItems.Any())
                {
                    TempData["ErrorMessage"] =
                        "Your cart is empty. Please select a service first.";

                    return RedirectToAction("Index", "Cart");
                }

                // =================================================
                // VALIDATE BOOKING DATE AND TIME
                // =================================================

                var today = DateTime.Today;

                if (!model.BookingDate.HasValue ||
                    model.BookingDate.Value.Date < today)
                {
                    ModelState.AddModelError(
                        nameof(model.BookingDate),
                        "Please select today or a future booking date.");
                }

                if (!model.BookingTime.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.BookingTime),
                        "Please select a booking time.");
                }
                else
                {
                    var time = model.BookingTime.Value;

                    // Available start times: 07:00 to 15:00.
                    if (time < new TimeSpan(7, 0, 0) ||
                        time > new TimeSpan(15, 0, 0) ||
                        time.Ticks % TimeSpan.TicksPerHour != 0)
                    {
                        ModelState.AddModelError(
                            nameof(model.BookingTime),
                            "Please select a valid start time " +
                            "between 7:00 AM and 3:00 PM.");
                    }

                    if (model.BookingDate.HasValue &&
                        model.BookingDate.Value.Date == today &&
                        time <= DateTime.Now.TimeOfDay)
                    {
                        ModelState.AddModelError(
                            nameof(model.BookingTime),
                            "Please select a future booking time.");
                    }
                }

                // =================================================
                // VALIDATE CUSTOMER NOTES
                // =================================================

                if (!ModelState.IsValid)
                {
                    model.IsAuthenticated = true;

                    model.AvailableAddresses =
                        customer.Addresses?
                            .Where(x => x.IsActive)
                            .ToList()
                        ?? new List<CustomerAddress>();

                    model.CartItems = cart.CartItems
                        .Where(x => x.ServiceOption != null)
                        .Select(x => new BookingCartItemVM
                        {
                            ServiceOptionId = x.ServiceOptionId,
                            ServiceName = x.ServiceOption!.Name,
                            Quantity = x.Quantity,
                            UnitPrice = x.ServiceOption.Price,
                            TotalPrice =
                                x.ServiceOption.Price * x.Quantity
                        })
                        .ToList();

                    model.Subtotal = model.CartItems.Sum(
                        x => x.TotalPrice);

                    model.Total = model.Subtotal;

                    return View(model);
                }

                // =================================================
                // VALIDATE CART ITEMS AND CURRENT SERVICE PRICES
                // =================================================

                if (cart.CartItems.Any(x =>
                    x.Quantity < 1 ||
                    x.ServiceOption == null ||
                    !x.ServiceOption.IsActive ||
                    x.ServiceOption.Price < 0))
                {
                    _logger.LogWarning(
                        "Booking rejected because the cart contains " +
                        "an invalid or inactive service. User: {UserId}",
                        userId);

                    TempData["ErrorMessage"] =
                        "One or more services in your cart are no " +
                        "longer available. Please review your cart.";

                    return RedirectToAction("Index", "Cart");
                }

                // =================================================
                // VALIDATE SELECTED SAVED ADDRESSES
                // =================================================

                var selectedAddressIds =
                    (model.SelectedAddressIds ?? new List<int>())
                    .Distinct()
                    .ToList();

                var customerAddresses = customer.Addresses
                    .Where(x =>
                        x.IsActive &&
                        selectedAddressIds.Contains(x.Id))
                    .ToList();

                if (customerAddresses.Count !=
                    selectedAddressIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(model.SelectedAddressIds),
                        "One or more selected addresses are invalid.");

                    model.IsAuthenticated = true;

                    model.AvailableAddresses = customer.Addresses
                        .Where(x => x.IsActive)
                        .ToList();

                    model.CartItems = cart.CartItems
                        .Select(x => new BookingCartItemVM
                        {
                            ServiceOptionId = x.ServiceOptionId,
                            ServiceName = x.ServiceOption!.Name,
                            Quantity = x.Quantity,
                            UnitPrice = x.ServiceOption.Price,
                            TotalPrice =
                                x.ServiceOption.Price * x.Quantity
                        })
                        .ToList();

                    model.Subtotal = model.CartItems.Sum(
                        x => x.TotalPrice);

                    model.Total = model.Subtotal;

                    return View(model);
                }

                // =================================================
                // BUILD BOOKING ITEMS FROM DATABASE VALUES
                // =================================================

                var bookingItems = cart.CartItems
                    .Select(item =>
                    {
                        var service = item.ServiceOption!;

                        return new BookingItem
                        {
                            ServiceOptionId = service.Id,
                            ServiceName = service.Name,
                            Quantity = item.Quantity,
                            UnitPrice = service.Price,
                            TotalPrice =
                                service.Price * item.Quantity
                        };
                    })
                    .ToList();

                var subtotal = bookingItems.Sum(
                    x => x.TotalPrice);

                // =================================================
                // CREATE THE BOOKING
                // =================================================

                var booking = new Booking
                {
                    BookingReference =
                        $"MAS-{Guid.NewGuid():N}"
                            .Substring(0, 12)
                            .ToUpperInvariant(),

                    CustomerId = customer.Id,

                    BookingDate = model.BookingDate!.Value.Date,

                    BookingTime = model.BookingTime!.Value,

                    CustomerNotes = model.CustomerNotes?.Trim(),

                    Subtotal = subtotal,

                    Total = subtotal,

                    Status = BookingStatus.Pending,

                    PaymentStatus = PaymentStatus.Pending,

                    CreatedDate = DateTime.Now,

                    UpdatedDate = DateTime.Now,

                    BookingItems = bookingItems,

                    Addresses = customerAddresses
                };

                // =================================================
                // ADD NEW ADDRESSES
                // =================================================

                foreach (var addressInput in
                         model.NewAddresses ?? new List<BookingAddressInputVM>())
                {
                    // Ignore completely blank address rows.
                    if (string.IsNullOrWhiteSpace(
                            addressInput.AddressName) &&
                        string.IsNullOrWhiteSpace(
                            addressInput.AddressLine1) &&
                        string.IsNullOrWhiteSpace(
                            addressInput.City) &&
                        string.IsNullOrWhiteSpace(
                            addressInput.Province))
                    {
                        continue;
                    }

                    // Validate partially completed addresses.
                    if (string.IsNullOrWhiteSpace(
                            addressInput.AddressName) ||
                        string.IsNullOrWhiteSpace(
                            addressInput.AddressLine1) ||
                        string.IsNullOrWhiteSpace(
                            addressInput.City) ||
                        string.IsNullOrWhiteSpace(
                            addressInput.Province))
                    {
                        ModelState.AddModelError(
                            nameof(model.NewAddresses),
                            "Please complete all required fields " +
                            "for each new address.");

                        model.IsAuthenticated = true;

                        model.AvailableAddresses = customer.Addresses
                            .Where(x => x.IsActive)
                            .ToList();

                        model.CartItems = cart.CartItems
                            .Select(x => new BookingCartItemVM
                            {
                                ServiceOptionId = x.ServiceOptionId,
                                ServiceName = x.ServiceOption!.Name,
                                Quantity = x.Quantity,
                                UnitPrice = x.ServiceOption.Price,
                                TotalPrice =
                                    x.ServiceOption.Price * x.Quantity
                            })
                            .ToList();

                        model.Subtotal = model.CartItems.Sum(
                            x => x.TotalPrice);

                        model.Total = model.Subtotal;

                        return View(model);
                    }

                    var newAddress = new CustomerAddress
                    {
                        CustomerId = customer.Id,

                        AddressName =
                            addressInput.AddressName.Trim(),

                        AddressLine1 =
                            addressInput.AddressLine1.Trim(),

                        AddressLine2 =
                            addressInput.AddressLine2?.Trim(),

                        City = addressInput.City.Trim(),

                        Province = addressInput.Province.Trim(),

                        PostalCode =
                            addressInput.PostalCode?.Trim(),

                        IsActive = true,

                        IsDefault = false,

                        CreatedDate = DateTime.Now,

                        UpdatedDate = DateTime.Now
                    };

                    booking.Addresses.Add(newAddress);

                    // CustomerAddress belongs to the customer and
                    // is also associated with the booking.
                    customer.Addresses.Add(newAddress);
                }

                // =================================================
                // SAVE BOOKING AND CLOSE ACTIVE CART
                // =================================================

                await _unitOfWork.Booking.AddAsync(booking);

                // Close the cart only after a successful booking save.
                cart.IsActive = false;

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Booking {BookingReference} created successfully " +
                    "for CustomerId {CustomerId}. Total: {Total}",
                    booking.BookingReference,
                    customer.Id,
                    booking.Total);

                TempData["SuccessMessage"] =
                    $"Your booking was submitted successfully. " +
                    $"Reference: {booking.BookingReference}";

                return RedirectToAction(
                    "Confirmation",
                    new { id = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while creating a booking.");

                TempData["ErrorMessage"] =
                    "We could not complete your booking. " +
                    "Please try again.";

                return RedirectToAction("Create");
            }
        }
        // =========================================================
        // BOOKING CONFIRMATION
        // =========================================================

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            try
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Challenge();
                }

                var booking = await _unitOfWork.Booking.GetAsync(
                    x => x.Id == id &&
                         x.Customer.ApplicationUserId == userId,
                    includeProperties: "Customer,BookingItems,Addresses");

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking confirmation not found for booking {BookingId}.",
                        id);

                    return NotFound();
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading confirmation for booking {BookingId}.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load your booking confirmation.";

                return RedirectToAction("Index", "Home");
            }
        }
    }
}