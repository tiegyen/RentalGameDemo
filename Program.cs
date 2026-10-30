
using Microsoft.EntityFrameworkCore;
using RentalGameDemo.Data;
using Microsoft.AspNetCore.Identity;
using RentalGameDemo.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register session services
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Initialize test data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var passwordHasher = new PasswordHasher<Customer>();

    // Create test Customer
    var customer = context.Customers
        .FirstOrDefault(c => c.Username == "testuser");

    if (customer == null)
    {
        customer = new Customer
        {
            Username = "testuser",
            Email = "testuser@gmail.com"
        };

        customer.PasswordHash = passwordHasher.HashPassword(
            customer,
            "Test@12345"
        );

        context.Customers.Add(customer);
    }

    // Create test Game
    var game = context.Games
        .FirstOrDefault(g => g.Title == "Test Game");

    if (game == null)
    {
        game = new Game
        {
            Title = "Test Game",
            Description = "A game for testing rental functionality.",
            Genre = "Action"
        };

        context.Games.Add(game);
    }

    context.SaveChanges();

    // Create test SteamAccount
    bool accountExists = context.SteamAccounts
        .Any(a => a.GameId == game.GameId);

    if (!accountExists)
    {
        var steamAccount = new SteamAccount
        {
            AccountName = "test_steam_account",
            AccountPassword = "SimulatedPassword123",
            VerificationCode = "123456",
            IsAvailable = true,
            GameId = game.GameId
        };

        context.SteamAccounts.Add(steamAccount);
    }

    context.SaveChanges();
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Configure default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
