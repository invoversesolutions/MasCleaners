using MasCleaners.DataConnection;
using MasCleaners.Interfaces;

namespace MasCleaners.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly SqlDbContext _dbContext;

        public IServiceCategory ServiceCategory { get; private set; }
        public IService Service { get; private set; }
        public IServiceOptions ServiceOptions { get; private set; }
        public IAddOns AddOns { get; private set; }
        public IServiceAddOn ServiceAddOn { get; private set; }

        public UnitOfWork(SqlDbContext dbContext)
        {
            _dbContext = dbContext;
            ServiceCategory = new ServiceCategoryRepository(_dbContext);
            Service = new ServiceRepository(_dbContext);
            ServiceOptions = new ServiceOptionsRepository(_dbContext);
            AddOns = new AddOnsRepository(_dbContext);
            ServiceAddOn = new ServiceAddOnRepository(_dbContext);
        }
        Task IUnitOfWork.CommitAsync() => _dbContext.SaveChangesAsync();
    }
}
