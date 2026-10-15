using MasCleaners.Models;

namespace MasCleaners.ViewModels.AdminDashboard
{
    public class AdminDashboardVM
    {
        public int TotalBookings { get; set; }

        public int PendingBookings { get; set; }

        public int ConfirmedBookings { get; set; }

        public int CompletedBookings { get; set; }

        public List<Booking> RecentBookings { get; set; } = new();
    }
}