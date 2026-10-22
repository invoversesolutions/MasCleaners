using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class BookingRescheduleRequestRepository : Repository<BookingRescheduleRequest>, IBookingRescheduleRequest
    {
        public BookingRescheduleRequestRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
