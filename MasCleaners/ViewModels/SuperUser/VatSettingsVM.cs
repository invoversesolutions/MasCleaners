
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels.SuperUser
{
    public class VatSettingsVM
    {
        public int Id { get; set; }

        [Display(Name = "VAT registered")]
        public bool IsVatRegistered { get; set; } = false;

        [Display(Name = "VAT registration number")]
        [StringLength(50)]
        public string? VatRegistrationNumber { get; set; }

        [Required]
        [Display(Name = "VAT rate (%)")]
        [Range(typeof(decimal), "0", "100",
            ErrorMessage = "VAT rate must be between 0 and 100.")]
        public decimal VatRate { get; set; } = 15m;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective date")]
        public DateTime VatEffectiveDate { get; set; }
            = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "Administrative notes")]
        public string? Notes { get; set; }
    }
}
