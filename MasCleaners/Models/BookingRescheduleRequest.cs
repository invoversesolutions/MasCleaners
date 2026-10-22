using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasCleaners.Models
{
    public class BookingRescheduleRequest
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        public Booking? Booking { get; set; }

        [Required]
        public DateTime RequestedBookingDateTime { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public RescheduleRequestStatus Status { get; set; }
            = RescheduleRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? AdminResponse { get; set; }
    }

    public enum RescheduleRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Declined = 3,
        Withdrawn = 4
    }
}