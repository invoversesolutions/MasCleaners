using MasCleaners.DataConnection;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class PayementRepository : Repository<Payment>, Interfaces.IPayment
    {
        public PayementRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
