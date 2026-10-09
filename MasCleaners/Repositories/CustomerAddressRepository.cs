using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class CustomerAddressRepository : Repository<CustomerAddress>, ICustomerAddress
    {
        public CustomerAddressRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
