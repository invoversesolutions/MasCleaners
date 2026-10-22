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
        public ICustomerAddress CustomerAddress { get; }

        public IEmployee Employee { get; }

        public IPayment Payment { get; }
        public IVatSetting VatSetting { get; }  
        public IInvoice Invoice { get; }
        public IInvoiceItem InvoiceItem { get; }
        public IBookingRescheduleRequest BookingRescheduleRequest { get; }

        Task CommitAsync();
    }
}
