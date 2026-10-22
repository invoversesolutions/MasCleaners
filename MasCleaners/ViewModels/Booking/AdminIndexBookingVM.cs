
using MasCleaners.Models;

namespace MasCleaners.ViewModels.Booking
{
    public class AdminIndexBookingVM
    {
        // General search
        public string? SearchTerm { get; set; }

        // Booking identification
        public int? BookingId { get; set; }
        public string? BookingReference { get; set; }

        // Customer search
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerType { get; set; }
        public string? CompanyName { get; set; }
        public string? VatNumber { get; set; }

        // Booking details
        public string? ServiceName { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? CustomerNotes { get; set; }
        public string? BookingTime { get; set; }

        // Booking date range
        public DateTime? BookingDateFrom { get; set; }
        public DateTime? BookingDateTo { get; set; }

        // Record creation date range
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }

        // Booking totals
        public decimal? MinimumTotal { get; set; }
        public decimal? MaximumTotal { get; set; }

        // Sorting
        public string SortBy { get; set; } = "newest";
        public string SortDirection { get; set; } = "desc";

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public int TotalResults { get; set; }
        public int TotalPages { get; set; }

        public List<MasCleaners.Models.Booking> Bookings { get; set; } = new();
    }
}
