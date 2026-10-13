using System.ComponentModel.DataAnnotations;

namespace MasCleaners.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // ======================================================
        // APPLICATION USER
        // ======================================================

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;


        // ======================================================
        // EMPLOYEE DETAILS
        // ======================================================

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? EmployeeNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }


        // ======================================================
        // STATUS
        // ======================================================

        public bool IsActive { get; set; } = true;


        // ======================================================
        // AUDIT
        // ======================================================

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}