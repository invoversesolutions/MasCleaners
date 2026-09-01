using MasCleaners.DataConnection;
using MasCleaners.Interfaces;

namespace MasCleaners.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly SqlDbContext _dbContext;

        public IServiceCategory ServiceCategory { get; private set; }
       
        public IServiceOptions ServiceOptions { get; private set; }
      

        public UnitOfWork(SqlDbContext dbContext)
        {
            _dbContext = dbContext;
            ServiceCategory = new ServiceCategoryRepository(_dbContext);
            ServiceOptions = new ServiceOptionsRepository(_dbContext);
        }
        Task IUnitOfWork.CommitAsync() => _dbContext.SaveChangesAsync();
    }
}
