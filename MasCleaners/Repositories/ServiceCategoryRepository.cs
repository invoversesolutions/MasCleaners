using MasCleaners.Models;
using MasCleaners.Interfaces;
using MasCleaners.DataConnection;

namespace MasCleaners.Repositories
{
    public class ServiceCategoryRepository : Repository<ServiceCategory>, IServiceCategory
    {
        public ServiceCategoryRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
