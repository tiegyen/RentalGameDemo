using Microsoft.EntityFrameworkCore;
using RentalGameDemo.Models;

namespace RentalGameDemo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entity database sets
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<SteamAccount> SteamAccounts { get; set; }
        public DbSet<RentalOrder> RentalOrders { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer 1 - 0..* RentalOrder
            modelBuilder.Entity<RentalOrder>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.RentalOrders)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Game 1 - 0..* RentalOrder
            modelBuilder.Entity<RentalOrder>()
                .HasOne(r => r.Game)
                .WithMany(g => g.RentalOrders)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Game 1 - 0..* SteamAccount
            modelBuilder.Entity<SteamAccount>()
                .HasOne(s => s.Game)
                .WithMany(g => g.SteamAccounts)
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // SteamAccount 1 - 0..* RentalOrder
            modelBuilder.Entity<RentalOrder>()
                .HasOne(r => r.SteamAccount)
                .WithMany(s => s.RentalOrders)
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // RentalOrder 1 - 0..1 Payment
            modelBuilder.Entity<RentalOrder>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.RentalOrder)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}