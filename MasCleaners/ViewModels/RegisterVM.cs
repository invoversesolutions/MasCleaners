using MasCleaners.Models;
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels
{
    public class RegisterVM : IValidatableObject
    {
        // ======================================================
        // PERSONAL DETAILS
        // ======================================================

        [Required]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;


        // ======================================================
        // CUSTOMER TYPE
        // ======================================================

        [Display(Name = "Account type")]
        public CustomerType CustomerType { get; set; }
            = CustomerType.Individual;

        [StringLength(200)]
        [Display(Name = "Company name")]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        [Display(Name = "VAT number (optional)")]
        public string? VatNumber { get; set; }


        // ======================================================
        // LOGIN DETAILS
        // ======================================================

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Your password must contain at least 8 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        // ======================================================
        // CONDITIONAL VALIDATION
        // ======================================================

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (CustomerType == CustomerType.Company &&
                string.IsNullOrWhiteSpace(CompanyName))
            {
                yield return new ValidationResult(
                    "Company name is required for a company account.",
                    new[] { nameof(CompanyName) });
            }
        }
    }
}