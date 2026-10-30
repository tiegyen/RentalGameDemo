using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using RentalGameDemo.Data;
using RentalGameDemo.Models;
using RentalGameDemo.Services;

namespace RentalGameDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Customer> _passwordHasher;
        private readonly AccountOperations _accountOperations;

        // UML Attribute
        public string currentSessionToken { get; private set; }
            = string.Empty;

        public AccountController(ApplicationDbContext context, AccountOperations accountOperations)
        {
            _context = context;
            _accountOperations = accountOperations;
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

            HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
            HttpContext.Session.SetString("Username", customer.Username);

            return true;
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (!authenticateCustomer(username, password))
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            int? selectedGameId = HttpContext.Session.GetInt32("SelectedGameId");

            if (selectedGameId != null)
            {
                return RedirectToAction("SelectDuration", "Rental");
            }

            return RedirectToAction("Dashboard", "Rental");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Catalog", "Rental");
        }

        // UML Operation
        [NonAction]
        public SteamAccount? checkAvailability(int gameId)
        {
            return _accountOperations.checkAvailability(gameId);
        }


    }


}