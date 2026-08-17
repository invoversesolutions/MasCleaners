using MasCleaners.Models;
using MasCleaners.Interfaces;
using MasCleaners.DataConnection;

namespace MasCleaners.Repositories
{
    public class AddOnsRepository : Repository<AddOn>, IAddOns
    {
        public AddOnsRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
