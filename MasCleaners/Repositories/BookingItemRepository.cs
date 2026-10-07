using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;
using NuGet.Protocol;

namespace MasCleaners.Repositories
{
    public class BookingItemRepository : Repository<BookingItem>, IBookingItemRepository
    {
        public BookingItemRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
