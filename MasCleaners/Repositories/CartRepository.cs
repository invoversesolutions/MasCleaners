using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class CartRepository : Repository<Cart>, ICart
    {
        public CartRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
