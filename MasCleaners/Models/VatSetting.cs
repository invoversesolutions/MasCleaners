using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasCleaners.Models
{
    public class VatSetting
    {
        public int Id { get; set; }

        // VAT registration configuration
        [Display(Name = "VAT Registered")]
        public bool IsVatRegistered { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "VAT Registration Number")]
        public string? VatRegistrationNumber { get; set; }

        [Range(typeof(decimal), "0", "100")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "VAT Rate (%)")]
        public decimal VatRate { get; set; } = 15.00m;

        [Display(Name = "VAT Registration Effective Date")]
        public DateTime? VatEffectiveDate { get; set; }

        // Configuration status
        [Display(Name = "Configuration Active")]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Audit information
        [StringLength(450)]
        public string? UpdatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}