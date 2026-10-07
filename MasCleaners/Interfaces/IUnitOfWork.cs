namespace MasCleaners.Interfaces
{
    public interface IUnitOfWork
    {
        public IServiceCategory ServiceCategory { get; }
       
        public IServiceOptions ServiceOptions { get; }
        public ICart Cart { get; }
        public ICartItem CartItem { get; }

        public ICustomerRepository Customer { get; }
        public IBookingRepository Booking { get; }
        public IBookingItemRepository BookingItem { get; }


        Task CommitAsync();
    }
}
