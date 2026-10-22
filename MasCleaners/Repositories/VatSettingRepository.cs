using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class VatSettingRepository : Repository<VatSetting>, IVatSetting
    {
        public VatSettingRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
