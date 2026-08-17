using MasCleaners.Models;
using MasCleaners.Interfaces;
using MasCleaners.DataConnection;

namespace MasCleaners.Repositories
{
    public class ServiceAddOnRepository : Repository<ServiceAddOn>, IServiceAddOn
    {
        public ServiceAddOnRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
