using MasCleaners.Models;

namespace MasCleaners.ViewModels.Customer
{
    public class CustomerPaymentHistoryItemVM
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public string BookingReference { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string? TransactionReference { get; set; }

        public DateTime PaymentDate { get; set; }
    }

    public class CustomerOutstandingPaymentItemVM
    {
        public int BookingId { get; set; }

        public string BookingReference { get; set; } = string.Empty;

        public decimal BookingTotal { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal BalanceDue { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string BookingStatus { get; set; } = string.Empty;
    }
}