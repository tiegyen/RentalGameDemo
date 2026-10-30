
using Microsoft.AspNetCore.Identity;
using RentalGameDemo.Models;

namespace RentalGameDemo.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Seed sample games individually.
            if (!context.Games.Any(g => g.Title == "Stardew Valley"))
            {
                context.Games.Add(new Game
                {
                    Title = "Stardew Valley",
                    Description = "A relaxing farming and life simulation game.",
                    Genre = "Simulation"
                });
            }

            if (!context.Games.Any(g => g.Title == "Hollow Knight"))
            {
                context.Games.Add(new Game
                {
                    Title = "Hollow Knight",
                    Description = "An atmospheric action-adventure game.",
                    Genre = "Action"
                });
            }

            if (!context.Games.Any(g => g.Title == "The Witcher 3"))
            {
                context.Games.Add(new Game
                {
                    Title = "The Witcher 3",
                    Description = "An open-world fantasy role-playing game.",
                    Genre = "RPG"
                });
            }

            context.SaveChanges();

            // Seed one simulated Steam account for each game.
            List<Game> games = context.Games.ToList();

            foreach (Game game in games)
            {
                bool accountExists = context.SteamAccounts
                    .Any(a => a.GameId == game.GameId);

                if (!accountExists)
                {
                    context.SteamAccounts.Add(new SteamAccount
                    {
                        AccountName = $"demo_account_{game.GameId}",
                        AccountPassword = "DemoPassword123",
                        VerificationCode = "123456",
                        IsAvailable = true,
                        GameId = game.GameId
                    });
                }
            }

            context.SaveChanges();

            // Seed a demo customer account.
            if (!context.Customers.Any(c => c.Username == "testuser"))
            {
                Customer customer = new Customer
                {
                    Username = "testuser",
                    Email = "testuser@example.com"
                };

                PasswordHasher<Customer> passwordHasher =
                    new PasswordHasher<Customer>();

                customer.PasswordHash = passwordHasher.HashPassword(
                    customer,
                    "Test@12345"
                );

                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }
    }
}
