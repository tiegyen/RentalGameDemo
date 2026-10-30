using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using RentalGameDemo.Data;
using RentalGameDemo.Models;

namespace RentalGameDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Customer> _passwordHasher;

        // UML Attribute
        public string currentSessionToken { get; private set; }
            = string.Empty;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        // UML Operation
        [NonAction]
        public bool authenticateCustomer(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            Customer? customer = _context.Customers
                .FirstOrDefault(c => c.Username == username);

            if (customer == null)
            {
                return false;
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    customer,
                    customer.PasswordHash,
                    password
                );

            if (result == PasswordVerificationResult.Failed)
            {
                return false;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                customer.PasswordHash =
                    _passwordHasher.HashPassword(customer, password);

                _context.SaveChanges();
            }

            // Generate a session token after successful authentication.
            currentSessionToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)
            );

            HttpContext.Session.SetString(
                "CurrentSessionToken",
                currentSessionToken
            );

            return true;
        }

        // UML Operation
        [NonAction]
        public SteamAccount? checkAvailability(int gameId)
        {
            SteamAccount? availableAccount = _context.SteamAccounts
                .FirstOrDefault(account =>
                    account.GameId == gameId
                    && account.IsAvailable
                );

            return availableAccount;
        }

        // Temporary test action for development
        [HttpGet]
        public IActionResult TestAccountOperations()
        {
            // Allow this test action only in Development.
            var environment = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>();

            if (!environment.IsDevelopment())
            {
                return NotFound();
            }

            // Test 1: Authenticate with correct credentials.
            bool validLogin = authenticateCustomer(
                "testuser",
                "Test@12345"
            );

            // Test 2: Authenticate with incorrect password.
            bool invalidLogin = authenticateCustomer(
                "testuser",
                "WrongPassword"
            );

            // Find the test game.
            Game? game = _context.Games
                .FirstOrDefault(g => g.Title == "Test Game");

            if (game == null)
            {
                return NotFound("Test Game was not found.");
            }

            // Test 3: Check availability for an existing game.
            SteamAccount? availableAccount =
                checkAvailability(game.GameId);

            // Test 4: Check availability for a non-existing game.
            SteamAccount? unavailableAccount =
                checkAvailability(-1);

            return Json(new
            {
                AuthenticationWithCorrectPassword = validLogin,
                AuthenticationWithWrongPassword = invalidLogin,
                AvailableAccountFound = availableAccount != null,
                UnavailableAccountReturnsNull = unavailableAccount == null
            });
        }


    }


}