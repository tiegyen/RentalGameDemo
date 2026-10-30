namespace RentalGameDemo.Models
{
    public class RentalOrder
    {
        // UML Attributes
        public int OrderId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;

        // EF Core Foreign Keys
        public int CustomerId { get; set; }
        public int GameId { get; set; }
        public int AccountId { get; set; }

        // EF Core Navigation Properties
        public Customer Customer { get; set; } = null!;
        public Game Game { get; set; } = null!;
        public SteamAccount SteamAccount { get; set; } = null!;

        public Payment? Payment { get; set; }

        // UML Operations
        public bool createOrder()
        {
            return StartTime < EndTime;
        }

        public void updateEndTime(DateTime newEndTime)
        {
            if (newEndTime <= StartTime)
            {
                throw new ArgumentException(
                    "The new end time must be later than the start time."
                );
            }

            EndTime = newEndTime;
        }

        public void expireOrder()
        {
            Status = "Expired";
        }

        public void reassignAccount(SteamAccount alternativeAccount)
        {
            ArgumentNullException.ThrowIfNull(alternativeAccount);

            SteamAccount = alternativeAccount;
            AccountId = alternativeAccount.AccountId;
        }
    }
}