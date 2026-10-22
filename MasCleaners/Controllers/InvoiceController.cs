using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.Controllers
{
    [Authorize(Roles = StaticRoles.Role_Super + "," + StaticRoles.Role_Admin)]
    public class InvoiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoiceController(
            IUnitOfWork unitOfWork,
            ILogger<InvoiceController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Invoice
        public async Task<IActionResult> Index()
        {
            try
            {
                var invoices = await _unitOfWork.Invoice.GetAllAsync(
                    includeProperties: "Customer,Booking");

                var result = invoices
                    .OrderByDescending(x => x.InvoiceDate)
                    .ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load invoices.");
                TempData["Error"] = "Unable to load invoices.";
                return View(new List<Invoice>());
            }
        }

        // GET: Invoice/FromBooking?bookingId=123
        // Opens the existing invoice or creates one for a paid booking.
        [HttpGet]
        public async Task<IActionResult> FromBooking(int bookingId)
        {
            if (bookingId <= 0)
                return BadRequest();

            try
            {
                var booking = await _unitOfWork.Booking
                    .GetAsync(
                        x => x.Id == bookingId,
                        includeProperties:
                            "Customer,Customer.ApplicationUser,BookingItems");

                if (booking == null)
                {
                    TempData["Error"] = "The booking could not be found.";
                    return RedirectToAction("Index", "Booking");
                }

                if (booking.PaymentStatus != PaymentStatus.Paid)
                {
                    TempData["Error"] =
                        "An invoice can only be generated for a paid booking.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = bookingId });
                }

                // Always return the existing invoice if one has already been created.
                var existingInvoice = await _unitOfWork.Invoice
                    .GetAsync(x => x.BookingId == bookingId);

                if (existingInvoice != null)
                {
                    return RedirectToAction(
                        nameof(Details),
                        new { id = existingInvoice.Id });
                }

                // Do not issue an invoice with an invalid total.
                if (booking.Total < 0)
                {
                    TempData["Error"] =
                        "The booking total is invalid. The invoice could not be created.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = bookingId });
                }

                // Select the VAT setting effective on the invoice date.
                var invoiceDate = DateTime.UtcNow;

                var vatSettings = await _unitOfWork.VatSetting.GetAllAsync();

                var vatSetting = vatSettings
                    .Where(x => x.VatEffectiveDate <= invoiceDate.Date)
                    .OrderByDescending(x => x.VatEffectiveDate)
                    .FirstOrDefault();

                var isVatRegistered =
                    vatSetting?.IsVatRegistered == true;

                var vatRate = isVatRegistered
                    ? vatSetting!.VatRate
                    : 0m;

                if (vatRate < 0m || vatRate >= 100m)
                {
                    _logger.LogError(
                        "Invalid VAT rate {VatRate} for booking {BookingId}.",
                        vatRate, bookingId);

                    TempData["Error"] =
                        "The VAT configuration is invalid. Please contact an administrator.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = bookingId });
                }

                // Booking prices are VAT-inclusive: VAT is extracted, not added.
                var grossTotal = Math.Round(
                    booking.Total, 2, MidpointRounding.AwayFromZero);

                var vatAmount = isVatRegistered
                    ? Math.Round(
                        grossTotal * vatRate / (100m + vatRate),
                        2,
                        MidpointRounding.AwayFromZero)
                    : 0m;

                var subtotalAmount = grossTotal - vatAmount;

                var customer = booking.Customer;

                if (customer == null)
                {
                    TempData["Error"] =
                        "The booking has no associated customer.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = bookingId });
                }

                var contactPerson = $"{customer.FirstName} {customer.LastName}".Trim();

                var customerName = customer.CustomerType == CustomerType.Company
                    ? string.IsNullOrWhiteSpace(contactPerson)
                        ? customer.CompanyName?.Trim() ?? "Company Customer"
                        : string.IsNullOrWhiteSpace(customer.CompanyName)
                            ? contactPerson
                            : $"{customer.CompanyName.Trim()} ({contactPerson})"
                    : string.IsNullOrWhiteSpace(contactPerson)
                        ? "Individual Customer"
                        : contactPerson;

                if (string.IsNullOrWhiteSpace(customerName))
                    customerName = "Customer";

                var invoice = new Invoice
                {
                    InvoiceNumber = await GenerateInvoiceNumberAsync(invoiceDate),
                    BookingId = booking.Id,
                    CustomerId = customer.Id,

                    // Snapshot customer details so future profile edits
                    // do not change an already-issued invoice.
                    CustomerName = customerName,
                    CustomerEmail = customer.ApplicationUser?.Email,
                    CustomerPhone = customer.PhoneNumber,
                    CustomerVatNumber = customer.VatNumber,

                    InvoiceDate = invoiceDate,
                    IssuedDate = invoiceDate,

                    SubtotalAmount = subtotalAmount,
                    DiscountAmount = 0m,
                    VatAmount = vatAmount,
                    TotalAmount = grossTotal,

                    IsVatRegisteredSnapshot = isVatRegistered,
                    VatRateSnapshot = vatRate,
                    VatEffectiveDateSnapshot =
                        isVatRegistered ? vatSetting!.VatEffectiveDate : null,
                    VatRegistrationNumberSnapshot =
                        isVatRegistered ? vatSetting!.VatRegistrationNumber : null,

                    PricesAreVatInclusive = true,
                    Status = InvoiceStatus.Issued,
                    PaymentStatus = InvoicePaymentStatus.Paid,

                    CreatedByUserId = _userManager.GetUserId(User)!,
                    CreatedDate = invoiceDate,

                    InvoiceItems = booking.BookingItems.Select(item =>
                    {
                        var quantity = item.Quantity;
                        var unitPrice = item.UnitPrice;
                        var lineTotal = Math.Round(
                            quantity * unitPrice,
                            2,
                            MidpointRounding.AwayFromZero);

                        var lineVat = isVatRegistered
                            ? Math.Round(
                                lineTotal * vatRate / (100m + vatRate),
                                2,
                                MidpointRounding.AwayFromZero)
                            : 0m;

                        var lineSubtotal = lineTotal - lineVat;

                        return new InvoiceItem
                        {
                            Description = item.ServiceName,
                            Quantity = quantity,
                            UnitPriceInclusive = unitPrice,
                            DiscountAmount = 0m,
                            LineSubtotal = lineSubtotal,
                            VatAmount = lineVat,
                            LineTotal = lineTotal,
                            VatRateSnapshot = vatRate,
                            IsVatApplicableSnapshot = isVatRegistered
                        };
                    }).ToList()
                };

                await _unitOfWork.Invoice.AddAsync(invoice);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Invoice {InvoiceNumber} created for booking {BookingId}.",
                    invoice.InvoiceNumber, bookingId);

                return RedirectToAction(
                    nameof(Details),
                    new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve or create an invoice for booking {BookingId}.",
                    bookingId);

                TempData["Error"] =
                    "An error occurred while opening the invoice.";

                return RedirectToAction(
                    "Details", "Booking", new { id = bookingId });
            }
        }

        // GET: Invoice/Details/123
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var invoice = await _unitOfWork.Invoice
                    .GetAsync(
                        x => x.Id == id,
                        includeProperties: "Customer,Booking,InvoiceItems");

                if (invoice == null)
                    return NotFound();

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load invoice {InvoiceId}.", id);
                TempData["Error"] = "Unable to load the invoice.";

                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> GenerateInvoiceNumberAsync(DateTime date)
        {
            var invoices = await _unitOfWork.Invoice.GetAllAsync();

            var existingNumbers = invoices
                .Select(x => x.InvoiceNumber)
                .ToHashSet();

            string invoiceNumber;

            do
            {
                invoiceNumber = Random.Shared
                    .Next(1_000_000, 10_000_000)
                    .ToString();
            }
            while (existingNumbers.Contains(invoiceNumber));

            return invoiceNumber;
        }
    }
}