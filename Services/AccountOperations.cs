using Microsoft.EntityFrameworkCore;
using RentalGameDemo.Data;
using RentalGameDemo.Models;

namespace RentalGameDemo.Services
{
    public class AccountOperations
    {

        private readonly ApplicationDbContext _context;

        public AccountOperations(ApplicationDbContext context)
        {
            _context = context;
        }

        public SteamAccount? checkAvailability(int gameId)
        {
            return _context.SteamAccounts.FirstOrDefault(
                account => account.GameId == gameId && account.IsAvailable);
        }

    }
}
