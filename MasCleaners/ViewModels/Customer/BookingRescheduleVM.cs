using System.ComponentModel.DataAnnotations;

namespace MasCleaners.ViewModels.Customer
{
    public class BookingRescheduleVM
    {
        public int BookingId { get; set; }

        public string? BookingReference { get; set; }

        public DateTime CurrentBookingDate { get; set; }

        public string? CurrentBookingTime { get; set; }

        [Required(ErrorMessage = "Please select your requested date and time.")]
        [Display(Name = "Requested date and time")]
        public DateTime? RequestedBookingDateTime { get; set; }

        [StringLength(500, ErrorMessage = "The reason cannot exceed 500 characters.")]
        [Display(Name = "Reason for rescheduling")]
        public string? Reason { get; set; }
    }
}