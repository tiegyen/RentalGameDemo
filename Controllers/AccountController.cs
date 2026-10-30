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




    }


}