using System.Collections.Generic;

namespace RentalGameDemo.Models
{
    public class SteamAccount
    {
        // UML Attributes
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountPassword { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }

        // EF Core Foreign Key
        public int GameId { get; set; }

        // EF Core Navigation Properties
        public Game Game { get; set; } = null!;

        public ICollection<RentalOrder> RentalOrders { get; set; }
            = new List<RentalOrder>();

        // UML Operations
        public bool reserveAccount()
        {
            if (!IsAvailable)
            {
                return false;
            }

            IsAvailable = false;
            return true;
        }

        public void releaseAccount()
        {
            IsAvailable = true;
        }

        public void updateAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }
    }
}