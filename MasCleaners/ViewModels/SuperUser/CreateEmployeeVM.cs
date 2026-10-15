
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels.SuperUser
{
    public class CreateEmployeeVM
    {
        // Employee details

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Employee Number")]
        public string? EmployeeNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        // Identity account details

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Employee Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new();
    }
}
