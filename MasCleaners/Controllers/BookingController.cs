using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels;
using MasCleaners.ViewModels.Booking;
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

        [Authorize(Roles = $"{StaticRoles.Role_Admin},{StaticRoles.Role_Super}")]
        [HttpGet]
        public async Task<IActionResult> AdminIndexBooking([FromQuery] AdminIndexBookingVM model)
        {
            _logger.LogInformation(
                "Admin booking search requested. Page: {PageNumber}, Search: {SearchTerm}",
                model.PageNumber,
                model.SearchTerm
            );

            try
            {
                // Normalise pagination values.
                model.PageNumber = Math.Max(1, model.PageNumber);

                var allowedPageSizes = new[] { 15, 25, 50, 100 };

                if (!allowedPageSizes.Contains(model.PageSize))
                {
                    model.PageSize = 15;
                }

                // Load booking records and related information.
                var allBookings = await _unitOfWork.Booking.GetAllAsync(
                    includeProperties: "Customer,Customer.ApplicationUser,BookingItems"
                );

                IEnumerable<MasCleaners.Models.Booking> query = allBookings;

                // --------------------------------------------------
                // GENERAL SEARCH
                // --------------------------------------------------

                if (!string.IsNullOrWhiteSpace(model.SearchTerm))
                {
                    var term = model.SearchTerm.Trim();

                    query = query.Where(b =>
                        b.BookingReference.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                        || b.Id.ToString().Contains(term)
                        || (b.Customer != null &&
                            (
                                $"{b.Customer.FirstName} {b.Customer.LastName}"
                                    .Contains(term, StringComparison.OrdinalIgnoreCase)
                                || (b.Customer.ApplicationUser?.Email ?? "")
                                    .Contains(term, StringComparison.OrdinalIgnoreCase)
                                || (b.Customer.PhoneNumber ?? "")
                                    .Contains(term, StringComparison.OrdinalIgnoreCase)
                                || (b.Customer.CompanyName ?? "")
                                    .Contains(term, StringComparison.OrdinalIgnoreCase)
                                || (b.Customer.VatNumber ?? "")
                                    .Contains(term, StringComparison.OrdinalIgnoreCase)
                            ))
                        || (b.CustomerNotes ?? "")
                            .Contains(term, StringComparison.OrdinalIgnoreCase)
                        || b.BookingItems.Any(item =>
                            (item.ServiceName ?? "")
                                .Contains(term, StringComparison.OrdinalIgnoreCase)
                        )
                        || b.Status.ToString().Contains(
                            term, StringComparison.OrdinalIgnoreCase
                        )
                        || b.PaymentStatus.ToString().Contains(
                            term, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                // --------------------------------------------------
                // BOOKING IDENTIFICATION
                // --------------------------------------------------

                if (model.BookingId.HasValue)
                {
                    query = query.Where(b => b.Id == model.BookingId.Value);
                }

                if (!string.IsNullOrWhiteSpace(model.BookingReference))
                {
                    var reference = model.BookingReference.Trim();

                    query = query.Where(b =>
                        b.BookingReference.Contains(
                            reference,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                // --------------------------------------------------
                // CUSTOMER FILTERS
                // --------------------------------------------------

                if (!string.IsNullOrWhiteSpace(model.CustomerName))
                {
                    var name = model.CustomerName.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        (
                            (b.Customer.FirstName ?? "").Contains(
                                name, StringComparison.OrdinalIgnoreCase
                            )
                            || (b.Customer.LastName ?? "").Contains(
                                name, StringComparison.OrdinalIgnoreCase
                            )
                            || $"{b.Customer.FirstName} {b.Customer.LastName}"
                                .Contains(name, StringComparison.OrdinalIgnoreCase)
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.CustomerEmail))
                {
                    var email = model.CustomerEmail.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        (b.Customer.ApplicationUser != null) &&
                        (b.Customer.ApplicationUser.Email ?? "").Contains(
                            email, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.CustomerPhone))
                {
                    var phone = model.CustomerPhone.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        (b.Customer.PhoneNumber ?? "").Contains(
                            phone, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.CustomerType))
                {
                    var customerType = model.CustomerType.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        b.Customer.CustomerType.ToString().Equals(
                            customerType, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    var company = model.CompanyName.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        (b.Customer.CompanyName ?? "").Contains(
                            company, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.VatNumber))
                {
                    var vatNumber = model.VatNumber.Trim();

                    query = query.Where(b =>
                        b.Customer != null &&
                        (b.Customer.VatNumber ?? "").Contains(
                            vatNumber, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                // --------------------------------------------------
                // BOOKING DETAIL FILTERS
                // --------------------------------------------------

                if (!string.IsNullOrWhiteSpace(model.ServiceName))
                {
                    var service = model.ServiceName.Trim();

                    query = query.Where(b =>
                        b.BookingItems.Any(item =>
                            (item.ServiceName ?? "").Contains(
                                service, StringComparison.OrdinalIgnoreCase
                            )
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.Status))
                {
                    var status = model.Status.Trim();

                    query = query.Where(b =>
                        b.Status.ToString().Equals(
                            status, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.PaymentStatus))
                {
                    var paymentStatus = model.PaymentStatus.Trim();

                    query = query.Where(b =>
                        b.PaymentStatus.ToString().Equals(
                            paymentStatus, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.CustomerNotes))
                {
                    var notes = model.CustomerNotes.Trim();

                    query = query.Where(b =>
                        (b.CustomerNotes ?? "").Contains(
                            notes, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.BookingTime))
                {
                    var bookingTime = model.BookingTime.Trim();

                    query = query.Where(b =>
                        b.BookingTime.ToString().Contains(
                            bookingTime, StringComparison.OrdinalIgnoreCase
                        )
                    );
                }

                // --------------------------------------------------
                // BOOKING DATE FILTERS
                // --------------------------------------------------

                if (model.BookingDateFrom.HasValue)
                {
                    var from = model.BookingDateFrom.Value.Date;

                    query = query.Where(b => b.BookingDate.Date >= from);
                }

                if (model.BookingDateTo.HasValue)
                {
                    var to = model.BookingDateTo.Value.Date;

                    query = query.Where(b => b.BookingDate.Date <= to);
                }

                // --------------------------------------------------
                // CREATED DATE FILTERS
                // --------------------------------------------------

                if (model.CreatedDateFrom.HasValue)
                {
                    var from = model.CreatedDateFrom.Value.Date;

                    query = query.Where(b => b.CreatedDate.Date >= from);
                }

                if (model.CreatedDateTo.HasValue)
                {
                    var toExclusive = model.CreatedDateTo.Value.Date.AddDays(1);

                    query = query.Where(b => b.CreatedDate < toExclusive);
                }

                // --------------------------------------------------
                // TOTAL FILTERS
                // --------------------------------------------------

                if (model.MinimumTotal.HasValue)
                {
                    query = query.Where(b =>
                        b.Total >= model.MinimumTotal.Value
                    );
                }

                if (model.MaximumTotal.HasValue)
                {
                    query = query.Where(b =>
                        b.Total <= model.MaximumTotal.Value
                    );
                }

                // --------------------------------------------------
                // SORTING
                // --------------------------------------------------

                var descending = !string.Equals(
                    model.SortDirection,
                    "asc",
                    StringComparison.OrdinalIgnoreCase
                );

                query = model.SortBy?.ToLowerInvariant() switch
                {
                    "oldest" => query.OrderBy(b => b.CreatedDate),

                    "bookingdate" => descending
                        ? query.OrderByDescending(b => b.BookingDate)
                               .ThenByDescending(b => b.BookingTime)
                        : query.OrderBy(b => b.BookingDate)
                               .ThenBy(b => b.BookingTime),

                    "customer" => descending
                        ? query.OrderByDescending(b => b.Customer!.LastName)
                               .ThenByDescending(b => b.Customer!.FirstName)
                        : query.OrderBy(b => b.Customer!.LastName)
                               .ThenBy(b => b.Customer!.FirstName),

                    "total" => descending
                        ? query.OrderByDescending(b => b.Total)
                        : query.OrderBy(b => b.Total),

                    "reference" => descending
                        ? query.OrderByDescending(b => b.BookingReference)
                        : query.OrderBy(b => b.BookingReference),

                    _ => descending
                        ? query.OrderByDescending(b => b.CreatedDate)
                        : query.OrderBy(b => b.CreatedDate)
                };

                // --------------------------------------------------
                // PAGINATION
                // --------------------------------------------------

                model.TotalResults = query.Count();

                model.TotalPages = (int)Math.Ceiling(
                    model.TotalResults / (double)model.PageSize
                );

                if (model.TotalPages > 0 && model.PageNumber > model.TotalPages)
                {
                    model.PageNumber = model.TotalPages;
                }

                model.Bookings = query
                    .Skip((model.PageNumber - 1) * model.PageSize)
                    .Take(model.PageSize)
                    .ToList();

                _logger.LogInformation(
                    "Admin booking search completed. Results: {ResultCount}, Page: {PageNumber}",
                    model.TotalResults,
                    model.PageNumber
                );

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load or search admin bookings."
                );

                TempData["ErrorMessage"] =
                    "Unable to load bookings. Please try again.";

                return View(new AdminIndexBookingVM());
            }
        }
        // =========================================================
        // ADMIN - BOOKING DETAILS
        // =========================================================
        [Authorize(Roles = $"{StaticRoles.Role_Admin},{StaticRoles.Role_Super}")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Loading booking details for BookingId {BookingId}.",
                    id);

                var booking = await _unitOfWork.Booking.GetAsync(
                    x => x.Id == id,
                    includeProperties:
                        "Customer,Customer.ApplicationUser,BookingItems,Addresses,Payments");

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking with ID {BookingId} was not found.",
                        id);

                    TempData["ErrorMessage"] =
                        "The requested booking could not be found.";

                    return RedirectToAction("AdminIndexBooking");
                }

                ViewData["AddPaymentVM"] = new AddPaymentVM
                {
                    BookingId = booking.Id,
                    BookingReference = booking.BookingReference,
                    BookingTotal = booking.Total,
                    PaymentDate = DateTime.Today
                };

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load details for BookingId {BookingId}.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load booking details. Please try again.";

                return RedirectToAction("AdminIndexBooking");
            }
        }

        [Authorize(Roles = $"{StaticRoles.Role_Admin},{StaticRoles.Role_Super}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Admin approval requested for BookingId {BookingId}.", id);

                var booking = await _unitOfWork.Booking.GetAsync(
                    b => b.Id == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking could not be found.";
                    return RedirectToAction(nameof(AdminIndexBooking));
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    TempData["ErrorMessage"] =
                        "Only pending bookings can be approved.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedDate = DateTime.Now;

               await _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Booking {BookingId} was approved and confirmed.", id);

                TempData["SuccessMessage"] =
                    $"Booking {booking.BookingReference} has been approved.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to approve BookingId {BookingId}.", id);

                TempData["ErrorMessage"] =
                    "Unable to approve the booking. Please try again.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }
        [Authorize(Roles = $"{StaticRoles.Role_Admin},{StaticRoles.Role_Super}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(
            int id,
            DateTime? bookingDate,
            TimeSpan? bookingTime)
        {
            try
            {
                _logger.LogInformation(
                    "Reschedule requested for BookingId {BookingId}.", id);

                var booking = await _unitOfWork.Booking.GetAsync(
                    b => b.Id == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking could not be found.";
                    return RedirectToAction(nameof(AdminIndexBooking));
                }

                if (booking.Status == BookingStatus.Completed ||
                    booking.Status == BookingStatus.Cancelled ||
                    booking.Status == BookingStatus.Declined)
                {
                    TempData["ErrorMessage"] =
                        "This booking can no longer be rescheduled.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                if (!bookingDate.HasValue || !bookingTime.HasValue)
                {
                    TempData["ErrorMessage"] =
                        "Please select both a booking date and time.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                var newDate = bookingDate.Value.Date;
                var newTime = bookingTime.Value;

                if (newDate < DateTime.Today)
                {
                    TempData["ErrorMessage"] =
                        "The booking date cannot be in the past.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                // MAS Cleaners booking hours: 07:00 to 15:00.
                if (newTime < TimeSpan.FromHours(7) ||
                    newTime > TimeSpan.FromHours(15) ||
                    newTime.Ticks % TimeSpan.TicksPerMinute != 0)
                {
                    TempData["ErrorMessage"] =
                        "Choose a valid booking time between 07:00 and 15:00.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                // Reject a time that has already passed today.
                if (newDate == DateTime.Today &&
                    newTime <= DateTime.Now.TimeOfDay)
                {
                    TempData["ErrorMessage"] =
                        "Choose a future time for today's booking.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                var previousDate = booking.BookingDate;
                var previousTime = booking.BookingTime;

                booking.BookingDate = newDate;
                booking.BookingTime = newTime;
                booking.UpdatedDate = DateTime.Now;

              await  _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Booking {BookingId} rescheduled from {OldDate} {OldTime} " +
                    "to {NewDate} {NewTime}.",
                    id, previousDate, previousTime, newDate, newTime);

                TempData["SuccessMessage"] =
                    $"Booking {booking.BookingReference} has been rescheduled.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to reschedule BookingId {BookingId}.", id);

                TempData["ErrorMessage"] =
                    "Unable to reschedule the booking. Please try again.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Booking completion requested for BookingId {BookingId}.", id);

                var booking = await _unitOfWork.Booking.GetAsync(
                    b => b.Id == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking could not be found.";
                    return RedirectToAction(nameof(AdminIndexBooking));
                }

                if (booking.Status != BookingStatus.InProgress &&
         booking.Status != BookingStatus.Confirmed)
                {
                    TempData["ErrorMessage"] =
                        "Only confirmed or in-progress bookings can be completed.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                booking.Status = BookingStatus.Completed;
                booking.UpdatedDate = DateTime.Now;

              await  _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Booking {BookingId} marked as completed.", id);

                TempData["SuccessMessage"] =
                    $"Booking {booking.BookingReference} has been completed.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to complete BookingId {BookingId}.", id);

                TempData["ErrorMessage"] =
                    "Unable to complete the booking. Please try again.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }

    }
}