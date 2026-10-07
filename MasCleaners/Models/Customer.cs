using System.ComponentModel.DataAnnotations;

namespace MasCleaners.Models
{
    public class Customer
    {
        public int Id { get; set; }

        // ======================================================
        // APPLICATION USER
        // ======================================================

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;


        // ======================================================
        // PERSONAL INFORMATION
        // ======================================================

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;


        // ======================================================
        // CONTACT INFORMATION
        // ======================================================

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;


        // ======================================================
        // ADDRESS
        // ======================================================

        [StringLength(150)]
        public string? AddressLine1 { get; set; }

        [StringLength(150)]
        public string? AddressLine2 { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }


        // ======================================================
        // STATUS
        // ======================================================

        public bool IsActive { get; set; } = true;


        // ======================================================
        // AUDIT
        // ======================================================

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;


        // ======================================================
        // NAVIGATION
        // ======================================================

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}