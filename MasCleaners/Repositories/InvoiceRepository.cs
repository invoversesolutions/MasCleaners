using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoice
    {
        public InvoiceRepository(SqlDbContext context) : base(context)
        {
        }
    }
 
}
