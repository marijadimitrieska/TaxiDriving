using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PaginationApi.Models;

namespace PaginationApi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaxiDrivings> TaxiDrivings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaxiDrivings>()
                .ToTable("data_small");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified
                ));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).updatedDate = DateTime.Now;

                if(entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).createdDate = DateTime.Now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
      
    }
}
