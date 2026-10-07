using System.ComponentModel.DataAnnotations;

namespace MasCleaners.Models
{
    public class BookingItem
    {
        public int Id { get; set; }


        // ======================================================
        // BOOKING
        // ======================================================

        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;


        // ======================================================
        // SERVICE OPTION
        // ======================================================

        public int ServiceOptionId { get; set; }

        public ServiceOption ServiceOption { get; set; } = null!;


        // ======================================================
        // SERVICE SNAPSHOT
        // ======================================================

        [Required]
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty;


        // ======================================================
        // QUANTITY
        // ======================================================

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;


        // ======================================================
        // PRICE SNAPSHOT
        // ======================================================

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}