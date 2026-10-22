using MasCleaners.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.DataConnection
{
    public class SqlDbContext : IdentityDbContext<ApplicationUser>
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }
        public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;

        public DbSet<ServiceOption> ServiceOptions { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }

        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<VatSetting> VatSettings { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        public DbSet<BookingRescheduleRequest> BookingRescheduleRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceOption>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ServiceOption)
                .WithMany()
                .HasForeignKey(ci => ci.ServiceOptionId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)");


            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // =========================================================
            // APPLICATION USER -> CUSTOMER
            // =========================================================

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(x => x.Customer)
                .WithOne(x => x.ApplicationUser)
                .HasForeignKey<Customer>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // APPLICATION USER - EMPLOYEE (ONE TO ONE)
            // ==================================================

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.Employee)
                .WithOne(employee => employee.ApplicationUser)
                .HasForeignKey<Employee>(
                    employee => employee.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================================================
            // EMPLOYEE NUMBER
            // ==================================================

            modelBuilder.Entity<Employee>()
                .HasIndex(employee => employee.EmployeeNumber)
                .IsUnique();

            // Retain your other existing entity relationships
            // and decimal precision configuration here.
            // =========================================================
            // CUSTOMER -> BOOKINGS
            // =========================================================

            modelBuilder.Entity<Customer>()
                .HasMany(x => x.Bookings)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // BOOKING -> BOOKING ITEMS
            // =========================================================

            modelBuilder.Entity<Booking>()
                .HasMany(x => x.BookingItems)
                .WithOne(x => x.Booking)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // BOOKING ITEM -> SERVICE OPTION
            // =========================================================

            modelBuilder.Entity<BookingItem>()
                .HasOne(x => x.ServiceOption)
                .WithMany()
                .HasForeignKey(x => x.ServiceOptionId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // DECIMAL PRECISION
            // =========================================================

            modelBuilder.Entity<Booking>()
                .Property(x => x.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(x => x.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookingItem>()
                .Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            // =========================================================
            // BOOKING <-> CUSTOMER ADDRESSES
            // Many-to-many relationship
            // =========================================================

            modelBuilder.Entity<Booking>()
                .HasMany(x => x.Addresses)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "BookingCustomerAddress",
                    right => right
                        .HasOne<CustomerAddress>()
                        .WithMany()
                        .HasForeignKey("CustomerAddressId")
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left
                        .HasOne<Booking>()
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("BookingId", "CustomerAddressId");
                    });
            modelBuilder.Entity<VatSetting>(entity =>
            {
                entity.Property(v => v.VatRate)
                    .HasPrecision(5, 2);

                entity.HasIndex(v => v.IsActive);
            });
            modelBuilder.Entity<BookingRescheduleRequest>()
    .HasOne(x => x.Booking)
    .WithMany()
    .HasForeignKey(x => x.BookingId)
    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}