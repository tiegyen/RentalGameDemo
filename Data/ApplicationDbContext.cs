
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
    }
}
