
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalGameDemo.Data;
using RentalGameDemo.Models;
using RentalGameDemo.Services;
using RentalGameDemo.ViewModels;

namespace RentalGameDemo.Controllers
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountOperations _accountOperations;
        private readonly PaymentGatewayAdapter _paymentGatewayAdapter;

        public int selectedDuration { get; set; }
        public int selectedGameId { get; set; }

        public RentalController(
            ApplicationDbContext context,
            AccountOperations accountOperations,
            PaymentGatewayAdapter paymentGatewayAdapter)
        {
            _context = context;
            _accountOperations = accountOperations;
            _paymentGatewayAdapter = paymentGatewayAdapter;
        }

        // UML Operation

        [HttpGet]
        public IActionResult SelectGame(int gameId)
        {
            Game? game = _context.Games
                .FirstOrDefault(g => g.GameId == gameId);

            if (game == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetInt32("SelectedGameId", gameId);

            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("SelectDuration", "Rental");
        }

        [NonAction]
        public RentalOrder? processRental()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? gameId = HttpContext.Session.GetInt32("SelectedGameId");
            int? duration = HttpContext.Session.GetInt32("SelectedDuration");

            if (customerId == null || gameId == null ||
                duration == null || duration <= 0)
            {
                return null;
            }

            selectedGameId = gameId.Value;
            selectedDuration = duration.Value;

            Game? selectedGame = _context.Games
                .FirstOrDefault(g => g.GameId == selectedGameId);

            if (selectedGame == null)
            {
                return null;
            }

            string gameDetails = selectedGame.getGameDetails();

            SteamAccount? availableAccount =
                _accountOperations.checkAvailability(selectedGameId);

            if (availableAccount == null)
            {
                return null;
            }

            bool reservationResult = availableAccount.reserveAccount();

            if (!reservationResult)
            {
                return null;
            }

            _context.SaveChanges();

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(selectedDuration);

            HttpContext.Session.SetInt32(
                "PendingGameId", selectedGameId);

            HttpContext.Session.SetInt32(
                "PendingAccountId", availableAccount.AccountId);

            HttpContext.Session.SetInt32(
                "PendingDuration", selectedDuration);

            return new RentalOrder
            {
                CustomerId = customerId.Value,
                GameId = selectedGameId,
                AccountId = availableAccount.AccountId,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Pending",
                Game = selectedGame,
                SteamAccount = availableAccount
            };
        }

        // HTTP Action
        [HttpPost]
        public IActionResult PrepareRental(int gameId, int duration)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            HttpContext.Session.SetInt32("SelectedGameId", gameId);
            HttpContext.Session.SetInt32("SelectedDuration", duration);

            selectedGameId = gameId;
            selectedDuration = duration;

            RentalOrder? pendingOrder = processRental();

            if (pendingOrder == null)
            {
                TempData["ErrorMessage"] =
                    "No available account or invalid rental information.";

                return RedirectToAction("SelectDuration");
            }

            // Demo-only pricing: 10,000 VND per rental hour.
            decimal amount = duration * 10000m;

            HttpContext.Session.SetString(
                "PendingAmount",
                amount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            );

            ViewBag.Amount = amount;
            ViewBag.Duration = duration;

            return View("RentalSummary", pendingOrder);
        }

        // UML Operation
        [NonAction]
        public void confirmRental()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? gameId = HttpContext.Session.GetInt32("PendingGameId");
            int? accountId = HttpContext.Session.GetInt32("PendingAccountId");
            int? duration = HttpContext.Session.GetInt32("PendingDuration");

            if (customerId == null || gameId == null ||
                accountId == null || duration == null)
                return;

            SteamAccount? account = _context.SteamAccounts
                .FirstOrDefault(a => a.AccountId == accountId.Value);

            if (account == null || account.IsAvailable)
                return;

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(duration.Value);

            RentalOrder order = new RentalOrder
            {
                CustomerId = customerId.Value,
                GameId = gameId.Value,
                AccountId = accountId.Value,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Active"
            };

            Payment payment = new Payment
            {
                Amount = _paymentGatewayAdapter.paymentAmount,
                PaymentStatus = "Paid",
                TransactionDate = DateTime.Now,
                RentalOrder = order
            };

            if (!payment.recordPayment() || !order.createOrder())
            {
                account.releaseAccount();
                account.updateAvailability(true);
                _context.SaveChanges();
                return;
            }

            _context.RentalOrders.Add(order);
            _context.Payments.Add(payment);
            _context.SaveChanges();

            ClearPendingRental();
            HttpContext.Session.Remove("SelectedDuration");
            HttpContext.Session.Remove("SelectedGameId");
        }

        // HTTP Action
        [HttpPost]
        public IActionResult ConfirmRental()
        {
            int? accountId = HttpContext.Session.GetInt32("PendingAccountId");
            string? amountText = HttpContext.Session.GetString("PendingAmount");

            if (accountId == null || amountText == null ||
                !decimal.TryParse(
                    amountText,
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal amount))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No pending rental was found."
                });
            }

            bool paymentResult = _paymentGatewayAdapter.requestPayment(amount);

            string transactionResult =
                _paymentGatewayAdapter.returnTransactionResult();

            if (!paymentResult || transactionResult != "Success")
            {
                SteamAccount? account = _context.SteamAccounts
                    .FirstOrDefault(a => a.AccountId == accountId.Value);

                if (account != null)
                {
                    account.releaseAccount();
                    account.updateAvailability(true);
                    _context.SaveChanges();
                }

                ClearPendingRental();

                return BadRequest(new
                {
                    success = false,
                    message = "Payment failed. The account reservation was released."
                });
            }

            confirmRental();

            // confirmRental clears PendingAccountId only after saving the order.
            bool rentalCreated =
                HttpContext.Session.GetInt32("PendingAccountId") == null;

            if (!rentalCreated)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Payment succeeded, but rental creation failed."
                });
            }

            return RedirectToAction("ActiveRentals");
        }

        private void ClearPendingRental()
        {
            HttpContext.Session.Remove("PendingGameId");
            HttpContext.Session.Remove("PendingAccountId");
            HttpContext.Session.Remove("PendingDuration");
            HttpContext.Session.Remove("PendingAmount");
        }

        [HttpPost]
        public IActionResult CancelRental()
        {
            int? accountId = HttpContext.Session.GetInt32("PendingAccountId");

            if (accountId != null)
            {
                SteamAccount? account = _context.SteamAccounts
                    .FirstOrDefault(a => a.AccountId == accountId.Value);

                if (account != null)
                {
                    account.releaseAccount();
                    account.updateAvailability(true);
                    _context.SaveChanges();
                }
            }

            ClearPendingRental();

            return RedirectToAction("Catalog");
        }

        // UML Operation
        [NonAction]
        public bool validateExtensionEligibility(int orderId)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            RentalOrder? order = _context.RentalOrders
                .FirstOrDefault(o =>
                    o.OrderId == orderId &&
                    o.CustomerId == customerId);

            if (order == null)
                return false;

            return order.Status == "Active" &&
                   DateTime.Now < order.EndTime;
        }

        [HttpGet]
        public IActionResult Catalog()
        {
            List<Game> games = _context.Games
                .Include(g => g.SteamAccounts)
                .OrderBy(g => g.Title)
                .ToList();

            return View("Catalog", games);
        }

        // UML Operation
        [NonAction]
        public bool processExtension(int orderId, int extensionDuration)
        {
            if (extensionDuration <= 0 ||
                !validateExtensionEligibility(orderId))
                return false;

            RentalOrder? order = _context.RentalOrders
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return false;

            DateTime newEndTime =
                order.EndTime.AddHours(extensionDuration);

            SteamAccount? currentAccount = _context.SteamAccounts
                .FirstOrDefault(a => a.AccountId == order.AccountId);

            if (currentAccount == null)
                return false;

            bool conflictingRental = _context.RentalOrders.Any(o =>
                o.OrderId != orderId &&
                o.AccountId == currentAccount.AccountId &&
                o.Status == "Active" &&
                o.StartTime < newEndTime &&
                o.EndTime > order.EndTime);

            if (!conflictingRental)
            {
                order.updateEndTime(newEndTime);
                _context.SaveChanges();
                return true;
            }

            SteamAccount? alternativeAccount = _context.SteamAccounts
                .FirstOrDefault(a =>
                    a.GameId == order.GameId &&
                    a.IsAvailable &&
                    a.AccountId != currentAccount.AccountId);

            if (alternativeAccount == null)
                return false;

            if (!alternativeAccount.reserveAccount())
                return false;

            order.reassignAccount(alternativeAccount);
            order.updateEndTime(newEndTime);

            currentAccount.releaseAccount();
            currentAccount.updateAvailability(true);

            _context.SaveChanges();

            return true;
        }

        // HTTP Action
        [HttpPost]
        public IActionResult RequestExtension(
            int orderId, int extensionDuration)
        {
            bool result = processExtension(orderId, extensionDuration);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Extension request was rejected."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Rental extension confirmed."
            });
        }

        // UML Operation
        [NonAction]
        public void processExpiration()
        {
            DateTime currentTime = DateTime.Now;

            List<RentalOrder> expiredOrders = _context.RentalOrders
                .Where(o =>
                    o.Status == "Active" &&
                    o.EndTime <= currentTime)
                .ToList();

            foreach (RentalOrder order in expiredOrders)
            {
                order.expireOrder();

                SteamAccount? account = _context.SteamAccounts
                    .FirstOrDefault(a => a.AccountId == order.AccountId);

                if (account != null)
                {
                    bool hasOtherActiveRental = _context.RentalOrders.Any(o =>
                        o.OrderId != order.OrderId &&
                        o.AccountId == account.AccountId &&
                        o.Status == "Active" &&
                        o.EndTime > currentTime);

                    if (!hasOtherActiveRental)
                    {
                        account.releaseAccount();
                        account.updateAvailability(true);
                    }
                }
            }

            _context.SaveChanges();
        }

        // HTTP Action
        [HttpPost]
        public IActionResult ProcessExpiration()
        {
            processExpiration();

            return Ok(new
            {
                success = true,
                message = "Rental expiration processing completed."
            });
        }

        [HttpGet]
        public IActionResult ActiveRentals()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");

            }

            processExpiration();

            RentalUI rentalUI = new RentalUI();

            rentalUI.activeRentalOrders = _context.RentalOrders
                .Include(o => o.Game)
                .Include(o => o.SteamAccount)
                .Where(o =>
                    o.CustomerId == customerId.Value &&
                    o.Status == "Active")
                .OrderBy(o => o.EndTime)
                .ToList();

            rentalUI.showActiveRentals();

            return View("ActiveRentals", rentalUI);
        }

        [HttpGet]
        public IActionResult SelectDuration()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? gameId = HttpContext.Session.GetInt32("SelectedGameId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (gameId == null)
            {
                return RedirectToAction("Catalog");
            }

            Game? game = _context.Games
                .Include(g => g.SteamAccounts)
                .FirstOrDefault(g => g.GameId == gameId.Value);

            if (game == null)
            {
                return NotFound();
            }

            return View("SelectDuration", game);
        }

    }
}
