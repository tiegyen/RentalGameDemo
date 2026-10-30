using RentalGameDemo.Models;
using System.Collections.Generic;

namespace RentalGameDemo.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public int ActiveRentalCount { get; set; }

        public int TotalRentalCount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public int TotalPaymentRecords { get; set; }

        public List<RentalOrder> ActiveRentals { get; set; }
            = new List<RentalOrder>();

        public List<Payment> PaymentHistory { get; set; }
            = new List<Payment>();
    }
}