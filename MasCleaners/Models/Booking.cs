using System.ComponentModel.DataAnnotations;

namespace MasCleaners.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // ======================================================
        // BOOKING REFERENCE
        // ======================================================

        [Required]
        [StringLength(50)]
        public string BookingReference { get; set; } = string.Empty;


        // ======================================================
        // CUSTOMER
        // ======================================================

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;


        // ======================================================
        // BOOKING DATE & TIME
        // ======================================================

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan BookingTime { get; set; }


        // ======================================================
        // CUSTOMER NOTES
        // ======================================================

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }


        // ======================================================
        // PRICING
        // ======================================================

        public decimal Subtotal { get; set; }

        public decimal Total { get; set; }


        // ======================================================
        // BOOKING STATUS
        // ======================================================

        public BookingStatus Status { get; set; }
            = BookingStatus.Pending;


        // ======================================================
        // PAYMENT
        // ======================================================

        public PaymentStatus PaymentStatus { get; set; }
            = PaymentStatus.Pending;


        // ======================================================
        // AUDIT
        // ======================================================

        public DateTime CreatedDate { get; set; }
            = DateTime.Now;

        public DateTime UpdatedDate { get; set; }
            = DateTime.Now;


        // ======================================================
        // BOOKING ADDRESSES
        // ======================================================

        public ICollection<CustomerAddress> Addresses { get; set; }
            = new List<CustomerAddress>();


        // ======================================================
        // BOOKING ITEMS
        // ======================================================

        public ICollection<BookingItem> BookingItems { get; set; }
            = new List<BookingItem>();
    }
}