using System.Collections.Generic;

namespace RentalGameDemo.Models
{
    public class Game
    {
        // UML Attributes
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;

        // EF Core Navigation Properties
        public ICollection<SteamAccount> SteamAccounts { get; set; }
            = new List<SteamAccount>();

        public ICollection<RentalOrder> RentalOrders { get; set; }
            = new List<RentalOrder>();

        // UML Operations
        public string getGameDetails()
        {
            return $"{Title} - {Genre}: {Description}";
        }
    }
}