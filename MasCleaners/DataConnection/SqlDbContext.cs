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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceOption>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

          
        }
    }
}