namespace MasCleaners.Interfaces
{
    public interface IUnitOfWork
    {
        public IServiceCategory ServiceCategory { get; }
       
        public IServiceOptions ServiceOptions { get; }
  

        Task CommitAsync();
    }
}
