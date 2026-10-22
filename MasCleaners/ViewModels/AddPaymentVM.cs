
using System;
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels
{
    public class AddPaymentVM
    {
        public int BookingId { get; set; }

        public string BookingReference { get; set; } = string.Empty;

        public decimal BookingTotal { get; set; }

        [Required(ErrorMessage = "Enter the payment amount.")]
        [Range(0.01, 999999999,
    ErrorMessage = "The payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Select a payment method.")]
        [RegularExpression("^(EFT|Card|Cash)$",
            ErrorMessage = "Select EFT, Card Payment, or Cash.")]
        public string PaymentMethod { get; set; } = "EFT";

        [StringLength(100, ErrorMessage = "The reference cannot exceed 100 characters.")]
        public string? TransactionReference { get; set; }

        [Required(ErrorMessage = "Select the payment date.")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }
    }
}
