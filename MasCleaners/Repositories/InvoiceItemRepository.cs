using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class InvoiceItemRepository : Repository<InvoiceItem>, IInvoiceItem
    {
        public InvoiceItemRepository(SqlDbContext context) : base(context)
        {
        }
    }
  
}
