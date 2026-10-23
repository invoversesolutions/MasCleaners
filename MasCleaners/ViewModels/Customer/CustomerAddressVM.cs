using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels.Customer
{
    public class CustomerAddressVM
    {
        // Null when creating a new address; populated when editing.
        public int? Id { get; set; }
        [Required(ErrorMessage = "Please provide a name for this address.")]
        [StringLength(100)]
        [Display(Name = "Address Name")]
        public string AddressName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line 1 is required.")]
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required.")]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Set as default address")]
        public bool IsDefault { get; set; }
    }
}