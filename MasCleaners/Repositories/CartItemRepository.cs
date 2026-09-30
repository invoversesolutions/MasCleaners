using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class CartItemRepository : Repository<CartItem>, ICartItem
    {
        public CartItemRepository(SqlDbContext context) : base(context)
        {
        }
    }
    
}
