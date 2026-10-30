using System.Collections.Generic;

namespace RentalGameDemo.Models
{
    public class Customer
    {
        // UML Attributes
        public int CustomerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // EF Core Navigation Property
        public ICollection<RentalOrder> RentalOrders { get; set; }
            = new List<RentalOrder>();

        // UML Operations
        public bool validateCredentials()
        {
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(PasswordHash);
        }

        public void requestRental()
        {
            // Rental request processing is handled by RentalController.
        }
    }
}