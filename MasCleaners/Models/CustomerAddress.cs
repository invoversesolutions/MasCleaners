
using System.ComponentModel.DataAnnotations;

namespace MasCleaners.Models
{
    public class CustomerAddress
    {
        public int Id { get; set; }

        // ======================================================
        // CUSTOMER
        // ======================================================

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;


        // ======================================================
        // ADDRESS LABEL
        // ======================================================

        [Required]
        [StringLength(100)]
        public string AddressName { get; set; } = string.Empty;
        // Examples: Home, Office, Rental Property


        // ======================================================
        // ADDRESS DETAILS
        // ======================================================

        [Required]
        [StringLength(150)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(150)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }


        // ======================================================
        // PREFERENCES
        // ======================================================

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;


        // ======================================================
        // AUDIT
        // ======================================================

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
