
using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{

    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(
            IUnitOfWork unitOfWork,
            ILogger<CustomerController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }
        [Authorize(Roles = $"{StaticRoles.Role_Admin},{StaticRoles.Role_Super},{StaticRoles.Role_Compliance}")]
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

        // GET: Customer/BookingHistory
        [HttpGet]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> BookingHistory()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Challenge();

                var customer = await _unitOfWork.Customer
                    .GetAsync(
                        x => x.ApplicationUserId == userId);

                if (customer == null)
                {
                    TempData["Error"] =
                        "Your customer profile could not be found.";

                    return View(new List<Booking>());
                }

                var bookings = await _unitOfWork.Booking.GetAllAsync(
                    x => x.CustomerId == customer.Id,
                    includeProperties: "BookingItems");

                var result = bookings
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load booking history for the signed-in customer.");

                TempData["Error"] =
                    "We could not load your bookings. Please try again.";

                return View(new List<Booking>());
            }
        }

        // GET: Customer/BookingDetails/123
        [HttpGet]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> BookingDetails(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Challenge();

                var customer = await _unitOfWork.Customer
                    .GetAsync(
                        x => x.ApplicationUserId == userId);

                if (customer == null)
                    return NotFound();

                // Ownership is checked in the database query itself.
                var booking = await _unitOfWork.Booking
                    .GetAsync(
                        x => x.Id == id && x.CustomerId == customer.Id,
                        includeProperties: "BookingItems");

                if (booking == null)
                    return NotFound();

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load booking {BookingId} for the signed-in customer.",
                    id);

                TempData["Error"] =
                    "We could not open this booking.";

                return RedirectToAction(nameof(BookingHistory));
            }
        }

        [HttpGet]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> UpcomingBookings()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Challenge();

                var customer = await _unitOfWork.Customer
                    .GetAsync(x => x.ApplicationUserId == userId);

                if (customer == null)
                {
                    TempData["Error"] = "Your customer profile could not be found.";
                    return View(new List<Booking>());
                }

                var bookings = await _unitOfWork.Booking.GetAllAsync(
                    x => x.CustomerId == customer.Id
                         && x.Status == BookingStatus.Confirmed
                         && x.BookingDate >= DateTime.Today,
                    includeProperties: "BookingItems");

                var upcomingBookings = bookings
                    .OrderBy(x => x.BookingDate)
                    .ThenBy(x => x.BookingTime)
                    .ToList();

                return View(upcomingBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load upcoming bookings for the signed-in customer.");

                TempData["Error"] = "We could not load your upcoming bookings.";

                return View(new List<Booking>());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Challenge();

                var customer = await _unitOfWork.Customer
                    .GetAsync(x => x.ApplicationUserId == userId);

                if (customer == null)
                    return NotFound();

                var booking = await _unitOfWork.Booking
                    .GetAsync(x => x.Id == id && x.CustomerId == customer.Id);

                if (booking == null)
                    return NotFound();

                if (booking.Status != BookingStatus.Confirmed)
                {
                    TempData["Error"] =
                        "Only confirmed bookings can be cancelled.";

                    return RedirectToAction(nameof(BookingDetails),
                        new { id = booking.Id });
                }

                if (booking.BookingDate.Date < DateTime.Today)
                {
                    TempData["Error"] =
                        "Past bookings cannot be cancelled online.";

                    return RedirectToAction(nameof(BookingDetails),
                        new { id = booking.Id });
                }

                booking.Status = BookingStatus.Cancelled;

              await  _unitOfWork.Booking.UpdateAsync(booking);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Customer {CustomerId} cancelled booking {BookingId}.",
                    customer.Id,
                    booking.Id);

                TempData["Success"] =
                    $"Booking {booking.BookingReference} has been cancelled.";

                return RedirectToAction(nameof(BookingDetails),
                    new { id = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to cancel booking {BookingId}.", id);

                TempData["Error"] =
                    "We could not cancel this booking. Please try again.";

                return RedirectToAction(nameof(BookingHistory));
            }
        }
        [HttpGet]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> RequestReschedule(int id)
        {
            if (id <= 0)
                return BadRequest();

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var customer = await _unitOfWork.Customer
                .GetAsync(x => x.ApplicationUserId == userId);

            if (customer == null)
                return NotFound();

            var booking = await _unitOfWork.Booking
                .GetAsync(x => x.Id == id && x.CustomerId == customer.Id);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Confirmed ||
                booking.BookingDate.Date < DateTime.Today)
            {
                TempData["Error"] =
                    "Only confirmed upcoming bookings can be rescheduled.";

                return RedirectToAction(nameof(BookingDetails),
                    new { id = booking.Id });
            }

            var existingRequest = await _unitOfWork.BookingRescheduleRequest
                .GetAsync(
                    x => x.BookingId == booking.Id &&
                         x.Status == RescheduleRequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["Error"] =
                    "You already have a pending reschedule request for this booking.";

                return RedirectToAction(nameof(BookingDetails),
                    new { id = booking.Id });
            }

            var vm = new BookingRescheduleVM
            {
                BookingId = booking.Id,
                BookingReference = booking.BookingReference,
                CurrentBookingDate = booking.BookingDate,
                CurrentBookingTime = booking.BookingTime.ToString()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> RequestReschedule(BookingRescheduleVM vm)
        {
            if (vm.BookingId <= 0)
                return BadRequest();

            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                    return Challenge();

                // Get the customer linked to the signed-in user.
                var customer = await _unitOfWork.Customer
                    .GetAsync(x => x.ApplicationUserId == userId);

                if (customer == null)
                    return NotFound();

                // Verify that this booking belongs to the customer.
                var booking = await _unitOfWork.Booking
                    .GetAsync(x =>
                        x.Id == vm.BookingId &&
                        x.CustomerId == customer.Id);

                if (booking == null)
                    return NotFound();

                // Revalidate booking eligibility on the server.
                if (booking.Status != BookingStatus.Confirmed ||
                    booking.BookingDate.Date < DateTime.Today)
                {
                    TempData["Error"] =
                        "Only confirmed upcoming bookings can be rescheduled.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id = booking.Id });
                }

                // Prevent multiple pending requests for the same booking.
                var existingRequest = await _unitOfWork.BookingRescheduleRequest
                    .GetAsync(x =>
                        x.BookingId == booking.Id &&
                        x.Status == RescheduleRequestStatus.Pending);

                if (existingRequest != null)
                {
                    TempData["Error"] =
                        "You already have a pending reschedule request for this booking.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id = booking.Id });
                }

                // Validate the requested date and time.
                if (!vm.RequestedBookingDateTime.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(vm.RequestedBookingDateTime),
                        "Please select your requested date and time.");
                }
                else if (vm.RequestedBookingDateTime.Value <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(vm.RequestedBookingDateTime),
                        "The requested date and time must be in the future.");
                }
                else if (vm.RequestedBookingDateTime.Value.Date ==
                             booking.BookingDate.Date &&
                         vm.RequestedBookingDateTime.Value.TimeOfDay ==
                             booking.BookingTime)
                {
                    ModelState.AddModelError(
                        nameof(vm.RequestedBookingDateTime),
                        "Please select a different date or time.");
                }

                if (!ModelState.IsValid)
                {
                    // Restore display-only values before returning the form.
                    vm.BookingReference = booking.BookingReference;
                    vm.CurrentBookingDate = booking.BookingDate;
                    vm.CurrentBookingTime = booking.BookingTime.ToString();

                    return View(vm);
                }

                var rescheduleRequest = new BookingRescheduleRequest
                {
                    BookingId = booking.Id,
                    RequestedBookingDateTime =
                        vm.RequestedBookingDateTime!.Value,
                    Reason = string.IsNullOrWhiteSpace(vm.Reason)
                        ? null
                        : vm.Reason.Trim(),
                    Status = RescheduleRequestStatus.Pending,
                    RequestedAt = DateTime.Now
                };

                await _unitOfWork.BookingRescheduleRequest
                    .AddAsync(rescheduleRequest);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Customer {CustomerId} submitted a reschedule request for booking {BookingId}.",
                    customer.Id,
                    booking.Id);

                TempData["Success"] =
                    "Your reschedule request has been submitted. Your existing booking remains unchanged until your request is reviewed.";

                return RedirectToAction(
                    nameof(BookingDetails),
                    new { id = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to submit a reschedule request for booking {BookingId}.",
                    vm.BookingId);

                TempData["Error"] =
                    "We could not submit your reschedule request. Please try again.";

                return RedirectToAction(
                    nameof(BookingDetails),
                    new { id = vm.BookingId });
            }
        }
        [HttpGet]
        [Authorize(Roles = $"{StaticRoles.Role_Customer}")]
        public async Task<IActionResult> ConfirmCancelBooking(int id)
        {
            if (id <= 0)
                return BadRequest();

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var customer = await _unitOfWork.Customer
                .GetAsync(x => x.ApplicationUserId == userId);

            if (customer == null)
                return NotFound();

            var booking = await _unitOfWork.Booking
                .GetAsync(
                    x => x.Id == id && x.CustomerId == customer.Id);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Confirmed ||
                booking.BookingDate.Date < DateTime.Today)
            {
                TempData["Error"] =
                    "This booking is no longer eligible for cancellation.";

                return RedirectToAction(nameof(BookingDetails),
                    new { id = booking.Id });
            }

            return View(booking);
        }
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var customer = await _unitOfWork.Customer
                    .GetAsync(c => c.ApplicationUserId == userId);

                if (customer == null)
                {
                    TempData["Error"] = "Your customer profile could not be found.";

                    return RedirectToAction(nameof(Index));
                }

                var addresses = await _unitOfWork.CustomerAddress
                    .GetAllAsync(
                        a => a.CustomerId == customer.Id && a.IsActive);

                return View(addresses
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.AddressName)
                    .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved customer addresses.");

                TempData["Error"] =
                    "We could not load your saved addresses. Please try again.";

                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public IActionResult CreateAddress()
        {
            return View(new CustomerAddressVM());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(CustomerAddressVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var customer = await _unitOfWork.Customer
                    .GetAsync(c => c.ApplicationUserId == userId);

                if (customer == null)
                {
                    TempData["Error"] = "Your customer profile could not be found.";

                    return RedirectToAction(nameof(Index));
                }

                var existingAddresses = await _unitOfWork.CustomerAddress
                    .GetAllAsync(a =>
                        a.CustomerId == customer.Id &&
                        a.IsActive);

                var addresses = existingAddresses.ToList();

                // The first active address becomes the default automatically.
                var makeDefault = model.IsDefault || !addresses.Any();

                if (makeDefault)
                {
                    foreach (var address in addresses.Where(a => a.IsDefault))
                    {
                        address.IsDefault = false;
                        address.UpdatedDate = DateTime.UtcNow;
                    }
                }

                var newAddress = new CustomerAddress
                {
                    CustomerId = customer.Id,

                    AddressName = model.AddressName.Trim(),
                    AddressLine1 = model.AddressLine1.Trim(),
                    AddressLine2 = string.IsNullOrWhiteSpace(model.AddressLine2)
                        ? null
                        : model.AddressLine2.Trim(),

                    City = model.City.Trim(),
                    Province = model.Province.Trim(),
                    PostalCode = model.PostalCode.Trim(),

                    IsDefault = makeDefault,
                    IsActive = true,

                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _unitOfWork.CustomerAddress.AddAsync(newAddress);

                await _unitOfWork.CommitAsync();

                TempData["Success"] = "Your address has been saved successfully.";

                return RedirectToAction(nameof(Addresses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving a customer address.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not save your address. Please try again.");

                return View(model);
            }
        }

        // GET: Customer/EditAddress/5
        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var customer = await _unitOfWork.Customer.GetAsync(
                c => c.ApplicationUserId == user.Id);

            if (customer == null)
                return NotFound();

            // Only load an active address belonging to the logged-in customer.
            var address = await _unitOfWork.CustomerAddress.GetAsync(
                a => a.Id == id
                     && a.CustomerId == customer.Id
                     && a.IsActive);

            if (address == null)
                return NotFound();

            var model = new CustomerAddressVM
            {
                Id = address.Id,
                AddressName = address.AddressName,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                Province = address.Province,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault
            };

            return View(model);
        }


        // POST: Customer/EditAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(CustomerAddressVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var customer = await _unitOfWork.Customer.GetAsync(
                c => c.ApplicationUserId == user.Id);

            if (customer == null)
                return NotFound();

            // Verify ownership again on POST.
            var address = await _unitOfWork.CustomerAddress.GetAsync(
                a => a.Id == model.Id
                     && a.CustomerId == customer.Id
                     && a.IsActive);

            if (address == null)
                return NotFound();

            // If this address is becoming the default, unset the others.
            if (model.IsDefault)
            {
                var otherAddresses = await _unitOfWork.CustomerAddress.GetAllAsync(
                    a => a.CustomerId == customer.Id
                         && a.IsActive
                         && a.Id != address.Id);

                foreach (var otherAddress in otherAddresses)
                {
                    if (otherAddress.IsDefault)
                    {
                        otherAddress.IsDefault = false;
                       await _unitOfWork.CustomerAddress.UpdateAsync(otherAddress);
                    }
                }
            }

            // Update the existing entity instead of trusting customer or
            // ownership information submitted by the browser.
            address.AddressName = model.AddressName;
            address.AddressLine1 = model.AddressLine1;
            address.AddressLine2 = model.AddressLine2;
            address.City = model.City;
            address.Province = model.Province;
            address.PostalCode = model.PostalCode;
            address.IsDefault = model.IsDefault;

           await _unitOfWork.CustomerAddress.UpdateAsync(address);

            await _unitOfWork.CommitAsync();

            TempData["success"] = "Your address has been updated successfully.";

            return RedirectToAction(nameof(Addresses));
        }
        // =========================================================
        // MY INVOICES
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var customer = await _unitOfWork.Customer.GetAsync(
                c => c.ApplicationUserId == user.Id);

            if (customer == null)
                return NotFound();

            var invoices = await _unitOfWork.Invoice.GetAllAsync(
                i => i.CustomerId == customer.Id,
                includeProperties: "Booking");

            var model = invoices
                .OrderByDescending(i => i.IssuedDate)
                .ToList();

            return View(model);
        }


        // =========================================================
        // PAYMENT HISTORY
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var customer = await _unitOfWork.Customer.GetAsync(
                c => c.ApplicationUserId == user.Id);

            if (customer == null)
                return NotFound();

            var bookings = await _unitOfWork.Booking.GetAllAsync(
                b => b.CustomerId == customer.Id,
                includeProperties: "Payments");

            var model = bookings
                .SelectMany(booking => booking.Payments.Select(payment =>
                    new CustomerPaymentHistoryItemVM
                    {
                        PaymentId = payment.Id,
                        BookingId = booking.Id,
                        BookingReference = booking.BookingReference,
                        Amount = payment.Amount,
                        PaymentMethod = payment.PaymentMethod,
                        TransactionReference = payment.TransactionReference,
                        PaymentDate = payment.PaymentDate
                    }))
                .OrderByDescending(payment => payment.PaymentDate)
                .ToList();

            return View(model);
        }


        // =========================================================
        // OUTSTANDING PAYMENTS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> OutstandingPayments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var customer = await _unitOfWork.Customer.GetAsync(
                c => c.ApplicationUserId == user.Id);

            if (customer == null)
                return NotFound();

            var bookings = await _unitOfWork.Booking.GetAllAsync(
                b => b.CustomerId == customer.Id
                     && b.Status != BookingStatus.Cancelled
                     && b.PaymentStatus != PaymentStatus.Paid,
                includeProperties: "Payments");

            var model = bookings
                .Select(booking =>
                {
                    var amountPaid = booking.Payments.Sum(p => p.Amount);
                    var balanceDue = Math.Max(0m, booking.Total - amountPaid);

                    return new CustomerOutstandingPaymentItemVM
                    {
                        BookingId = booking.Id,
                        BookingReference = booking.BookingReference,
                        BookingTotal = booking.Total,
                        AmountPaid = amountPaid,
                        BalanceDue = balanceDue,
                        PaymentStatus = booking.PaymentStatus,
                        BookingStatus = booking.Status.ToString()
                    };
                })
                .Where(item => item.BalanceDue > 0)
                .OrderByDescending(item => item.BalanceDue)
                .ToList();

            return View(model);
        }
    }
}
