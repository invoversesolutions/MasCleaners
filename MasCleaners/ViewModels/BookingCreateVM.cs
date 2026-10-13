using System.ComponentModel.DataAnnotations;
using MasCleaners.Models;

namespace MasCleaners.ViewModels
{
    // =========================================================
    // BOOKING CREATE VIEW MODEL
    // =========================================================

    public class BookingCreateVM
    {
        // =====================================================
        // CUSTOMER INFORMATION
        // =====================================================

        public bool IsAuthenticated { get; set; }

        public CustomerType CustomerType { get; set; }
            = CustomerType.Individual;

        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your phone number.")]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? VatNumber { get; set; }


        // =====================================================
        // BOOKING SCHEDULE
        // =====================================================

        [Required(ErrorMessage = "Please select a booking date.")]
        public DateTime? BookingDate { get; set; }

        [Required(ErrorMessage = "Please select a booking time.")]
        public TimeSpan? BookingTime { get; set; }

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }


        // =====================================================
        // EXISTING SAVED ADDRESSES
        // =====================================================

        // Display data only; never trust posted address objects.
        public List<CustomerAddress> AvailableAddresses { get; set; }
            = new();

        // IDs selected by the customer.
        public List<int> SelectedAddressIds { get; set; }
            = new();


        // =====================================================
        // NEW ADDRESSES
        // =====================================================

        public List<BookingAddressInputVM> NewAddresses { get; set; }
            = new();


        // =====================================================
        // CART SUMMARY
        // =====================================================

        public List<BookingCartItemVM> CartItems { get; set; }
            = new();

        public decimal Subtotal { get; set; }

        public decimal Total { get; set; }
    }


    // =========================================================
    // BOOKING ADDRESS INPUT
    // =========================================================

    public class BookingAddressInputVM
    {
        [Required(ErrorMessage = "Please enter an address name.")]
        [StringLength(100)]
        public string AddressName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the street address.")]
        [StringLength(150)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(150)]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Please enter the city.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the province.")]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Logged-in customers may save new addresses
        // to their address book.
        public bool SaveToAddressBook { get; set; } = true;
    }


    // =========================================================
    // CART ITEM DISPLAY
    // =========================================================

    public class BookingCartItemVM
    {
        public int ServiceOptionId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}