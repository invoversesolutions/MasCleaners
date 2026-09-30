using MasCleaners.Models;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.DataConnection
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }
        public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
       
        public DbSet<ServiceOption> ServiceOptions { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

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

        }
    }
}