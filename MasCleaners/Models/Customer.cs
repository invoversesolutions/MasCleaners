
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
        // CUSTOMER TYPE
        // ======================================================

        public CustomerType CustomerType { get; set; }
            = CustomerType.Individual;


        // ======================================================
        // CUSTOMER / CONTACT DETAILS
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


        // ======================================================
        // COMPANY DETAILS
        // Required by application validation for companies
        // ======================================================

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? VatNumber { get; set; }


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

        public ICollection<CustomerAddress> Addresses { get; set; }
            = new List<CustomerAddress>();

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
