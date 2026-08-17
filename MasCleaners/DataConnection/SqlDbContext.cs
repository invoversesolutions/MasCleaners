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
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServiceOption> ServiceOptions { get; set; } = null!;
        public DbSet<AddOn> AddOns { get; set; } = null!;
        public DbSet<ServiceAddOn> ServiceAddOns { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceOption>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AddOn>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceAddOn>()
                .HasKey(x => new
                {
                    x.ServiceId,
                    x.AddOnId
                });

            modelBuilder.Entity<ServiceAddOn>()
                .HasOne(x => x.Service)
                .WithMany(x => x.ServiceAddOns)
                .HasForeignKey(x => x.ServiceId);

            modelBuilder.Entity<ServiceAddOn>()
                .HasOne(x => x.AddOn)
                .WithMany(x => x.ServiceAddOns)
                .HasForeignKey(x => x.AddOnId);
        }
    }
}