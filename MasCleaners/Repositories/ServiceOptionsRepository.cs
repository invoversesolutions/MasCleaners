using MasCleaners.Models;
using MasCleaners.Interfaces;
using MasCleaners.DataConnection;

namespace MasCleaners.Repositories
{
    public class ServiceOptionsRepository : Repository<ServiceOption>, IServiceOptions
    {
        public ServiceOptionsRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
