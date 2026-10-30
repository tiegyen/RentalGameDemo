namespace RentalGameDemo.Services
{
    public class PaymentGatewayAdapter
    {
        public string transactionId { get; set; } = string.Empty;

        public decimal paymentAmount { get; set; }

        public string gatewayResponseStatus { get; set; } = string.Empty;

        public bool requestPayment(decimal amount)
        {
            paymentAmount = amount;

            if (amount <= 0)
            {
                gatewayResponseStatus = "Failed";
                return false;
            }

            // Simulate a successful payment gateway response.
            transactionId = Guid.NewGuid().ToString();
            gatewayResponseStatus = "Success";

            return true;
        }

        public string returnTransactionResult()
        {
            return gatewayResponseStatus;
        }
    }
}