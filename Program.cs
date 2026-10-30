
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalGameDemo.Data;
using RentalGameDemo.Models;
using RentalGameDemo.Services;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register session services
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<AccountOperations>();
builder.Services.AddScoped<PaymentGatewayAdapter>();
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

// Initialize database sample data.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    DbInitializer.Initialize(context);
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
    pattern: "{controller=Rental}/{action=Catalog}/{id?}");

app.Run();
