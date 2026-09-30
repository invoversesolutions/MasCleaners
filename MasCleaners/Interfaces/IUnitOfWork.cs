namespace MasCleaners.Interfaces
{
    public interface IUnitOfWork
    {
        public IServiceCategory ServiceCategory { get; }
       
        public IServiceOptions ServiceOptions { get; }
        public ICart Cart { get; }
        public ICartItem CartItem { get; }


        Task CommitAsync();
    }
}
