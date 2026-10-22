
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasCleaners.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }

        // Optional links to the original catalogue records.
        public int? ServiceId { get; set; }

        public int? ServiceOptionId { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Range(0.001, 100000)]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; } = 1m;

        // Frozen inclusive price at the time of invoicing.
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceInclusive { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        // VAT-exclusive line value when registered;
        // otherwise equal to the final line amount.
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineSubtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal VatRateSnapshot { get; set; }

        public bool IsVatApplicableSnapshot { get; set; }
    }
}
