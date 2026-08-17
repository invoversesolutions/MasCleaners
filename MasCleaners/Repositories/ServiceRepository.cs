using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class ServiceRepository : Repository<Service>, IService
    {
        public ServiceRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
