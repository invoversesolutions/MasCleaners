using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using MasCleaners.Models;

namespace MasCleaners.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployee
    {
        public EmployeeRepository(SqlDbContext context) : base(context)
        {
        }
    }
}
