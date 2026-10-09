using MasCleaners.DataConnection;
using MasCleaners.Interfaces;

namespace MasCleaners.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly SqlDbContext _dbContext;

        public IServiceCategory ServiceCategory { get; private set; }
       
        public IServiceOptions ServiceOptions { get; private set; }
        public ICart Cart { get; private set; }
        public ICartItem CartItem { get; private set; }

        public ICustomerRepository Customer { get; private set; }
        public IBookingRepository Booking { get; private set; }
        public IBookingItemRepository BookingItem { get; private set; }

        public ICustomerAddress CustomerAddress { get; private set; }


        public UnitOfWork(SqlDbContext dbContext)
        {
            _dbContext = dbContext;
            ServiceCategory = new ServiceCategoryRepository(_dbContext);
            ServiceOptions = new ServiceOptionsRepository(_dbContext);
            Cart = new CartRepository(_dbContext);
            CartItem = new CartItemRepository(_dbContext);
            Customer = new CustomerRepository(_dbContext);
            Booking = new BookingRepository(_dbContext);
            BookingItem = new BookingItemRepository(_dbContext);
            CustomerAddress = new CustomerAddressRepository(_dbContext);
        }
        Task IUnitOfWork.CommitAsync() => _dbContext.SaveChangesAsync();
    }
}
