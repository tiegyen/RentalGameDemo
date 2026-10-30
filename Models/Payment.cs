namespace RentalGameDemo.Models
{
    public class Payment
    {
        // UML Attributes
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }

        // EF Core Foreign Key
        public int OrderId { get; set; }

        // EF Core Navigation Property
        public RentalOrder RentalOrder { get; set; } = null!;

        // UML Operations
        public bool recordPayment()
        {
            return Amount > 0
                && !string.IsNullOrWhiteSpace(PaymentStatus)
                && TransactionDate != default;
        }
    }
}