using MasCleaners.Models;
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels.Customer
{
    public class CustomerPersonalDetailsVM
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public CustomerType CustomerType { get; set; }

        // Company details
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

 

        [StringLength(50)]
        [Display(Name = "VAT Number")]
        public string? VatNumber { get; set; }
    }

    public class CustomerSecuritySettingsVM
    {
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }

        public bool HasPassword { get; set; }

        public bool TwoFactorEnabled { get; set; }
    }

    public class CustomerChangePasswordVM
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Your new password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword),
            ErrorMessage = "The new password and confirmation do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}