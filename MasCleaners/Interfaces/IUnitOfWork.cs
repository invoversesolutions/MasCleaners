namespace MasCleaners.Interfaces
{
    public interface IUnitOfWork
    {
        public IServiceCategory ServiceCategory { get; }
        public IService Service { get; }
        public IServiceOptions ServiceOptions { get; }
        public IAddOns AddOns { get; }
        public IServiceAddOn ServiceAddOn { get; }

        Task CommitAsync();
    }
}
