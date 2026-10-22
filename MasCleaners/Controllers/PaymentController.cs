
using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IUnitOfWork unitOfWork,
            ILogger<PaymentController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddPaymentVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please check the payment information and try again.";

                return RedirectToAction(
                    "Details", "Booking", new { id = model.BookingId });
            }

            try
            {
                _logger.LogInformation(
                    "Payment recording requested for BookingId {BookingId}.",
                    model.BookingId);

                var booking = await _unitOfWork.Booking.GetAsync(
                    b => b.Id == model.BookingId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] =
                        "The booking could not be found.";

                    return RedirectToAction(
                        "AdminIndexBooking", "Booking");
                }

                if (booking.Status == BookingStatus.Cancelled ||
                    booking.Status == BookingStatus.Declined)
                {
                    TempData["ErrorMessage"] =
                        "Payments cannot be recorded against cancelled or declined bookings.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = booking.Id });
                }

                if (!Enum.TryParse<PaymentMethod>(
                        model.PaymentMethod, true, out var paymentMethod) ||
                    !Enum.IsDefined(typeof(PaymentMethod), paymentMethod))
                {
                    TempData["ErrorMessage"] =
                        "Please select a valid payment method.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = booking.Id });
                }

                if (model.PaymentDate.Date > DateTime.Today)
                {
                    TempData["ErrorMessage"] =
                        "The payment date cannot be in the future.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = booking.Id });
                }

                // Load persisted payments, not values submitted by the browser.
                var existingPayments =
                    await _unitOfWork.Payment.GetAllAsync(
                        p => p.BookingId == booking.Id);

                var amountPaid = existingPayments.Sum(p => p.Amount);
                var outstandingBalance = booking.Total - amountPaid;

                if (outstandingBalance <= 0)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    booking.UpdatedDate = DateTime.Now;

                  await  _unitOfWork.Booking.UpdateAsync(booking);
                    await _unitOfWork.CommitAsync();

                    TempData["ErrorMessage"] =
                        "This booking has already been fully paid.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = booking.Id });
                }

                if (model.Amount <= 0 ||
                    model.Amount > outstandingBalance)
                {
                    TempData["ErrorMessage"] =
                        $"The payment must be greater than zero and cannot exceed the outstanding balance of R {outstandingBalance:N2}.";

                    return RedirectToAction(
                        "Details", "Booking", new { id = booking.Id });
                }

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = model.Amount,
                    PaymentMethod = paymentMethod,
                    TransactionReference =
                        string.IsNullOrWhiteSpace(model.TransactionReference)
                            ? null
                            : model.TransactionReference.Trim(),
                    PaymentDate = model.PaymentDate.Date,
                    Notes = string.IsNullOrWhiteSpace(model.Notes)
                        ? null
                        : model.Notes.Trim(),
                    CreatedDate = DateTime.Now
                };

               await _unitOfWork.Payment.AddAsync(payment);

                var newTotalPaid = amountPaid + payment.Amount;

                booking.PaymentStatus =
                    newTotalPaid >= booking.Total
                        ? PaymentStatus.Paid
                        : PaymentStatus.PartiallyPaid;

                booking.UpdatedDate = DateTime.Now;

              await  _unitOfWork.Booking.UpdateAsync(booking);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Payment {PaymentAmount} recorded for BookingId {BookingId} using {PaymentMethod}. New payment status: {PaymentStatus}.",
                    payment.Amount,
                    booking.Id,
                    paymentMethod,
                    booking.PaymentStatus);

                TempData["SuccessMessage"] =
                    $"Payment of R {payment.Amount:N2} recorded successfully. " +
                    $"Outstanding balance: R {Math.Max(0, booking.Total - newTotalPaid):N2}.";

                return RedirectToAction(
                    "Details", "Booking", new { id = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to record payment for BookingId {BookingId}.",
                    model.BookingId);

                TempData["ErrorMessage"] =
                    "Unable to record the payment. Please try again.";

                return RedirectToAction(
                    "Details", "Booking", new { id = model.BookingId });
            }
        }
    }
}
