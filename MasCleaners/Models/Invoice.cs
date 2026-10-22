
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasCleaners.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int? BookingId { get; set; }

        public Booking? Booking { get; set; }

        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        // Customer/business details captured at issue time.
        [Required, StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CustomerEmail { get; set; }

        [StringLength(30)]
        public string? CustomerPhone { get; set; }

        [StringLength(500)]
        public string? BillingAddress { get; set; }

        [StringLength(100)]
        public string? CustomerVatNumber { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public DateTime? IssuedDate { get; set; }

        // All monetary values are stored in ZAR.
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        // Amount extracted from VAT-inclusive prices.
        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Immutable tax snapshot.
        public bool IsVatRegisteredSnapshot { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal VatRateSnapshot { get; set; }

        public DateTime? VatEffectiveDateSnapshot { get; set; }

        [StringLength(100)]
        public string? VatRegistrationNumberSnapshot { get; set; }

        public bool PricesAreVatInclusive { get; set; } = true;

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public InvoicePaymentStatus PaymentStatus { get; set; }
            = InvoicePaymentStatus.Unpaid;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
            = new List<InvoiceItem>();
    }

    public enum InvoiceStatus
    {
        Draft = 0,
        Issued = 1,
        Void = 2
    }

    public enum InvoicePaymentStatus
    {
        Unpaid = 0,
        PartiallyPaid = 1,
        Paid = 2,
        Overdue = 3,
        Refunded = 4
    }
}
