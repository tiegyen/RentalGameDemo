
using RentalGameDemo.Models;
using System.Collections.Generic;

namespace RentalGameDemo.ViewModels
{
    public class RentalUI
    {
        // UML Attributes
        public string rentalDetailsView { get; set; } = string.Empty;

        public string maskedPassword { get; set; } = "********";

        public string verificationCodeDisplay { get; set; } = string.Empty;

        // Supporting data for the Active Rental Dashboard
        public List<RentalOrder> activeRentalOrders { get; set; }
            = new List<RentalOrder>();

        public int selectedRentalOrderId { get; set; }

        // UML Operations
        public void displayCatalog()
        {
            rentalDetailsView = "Game Catalog";
        }

        public void showActiveRentals()
        {
            rentalDetailsView = "Active Rentals";
        }

        public void unmaskPassword()
        {
            maskedPassword = string.Empty;
        }

        public void copyCredentials()
        {
            rentalDetailsView = "Credentials ready to copy.";
        }

        public void displayVerificationCode()
        {
            verificationCodeDisplay = "Verification code available.";
        }

        public void selectGame(int gameId)
        {
            selectedRentalOrderId = gameId;
        }

        public void displayRentalSummary(decimal amount)
        {
            rentalDetailsView = $"Rental Amount: {amount:N2}";
        }

        public void confirmRental()
        {
            rentalDetailsView = "Rental confirmation requested.";
        }

        public void rentalConfirmation()
        {
            rentalDetailsView = "Rental completed successfully.";
        }

        public void paymentFailed()
        {
            rentalDetailsView = "Payment failed or was cancelled.";
        }

        public void displayUnavailableMessage()
        {
            rentalDetailsView = "The selected game is currently unavailable.";
        }

        public void requestExtension(int orderId, int extensionDuration)
        {
            selectedRentalOrderId = orderId;
            rentalDetailsView =
                $"Extension requested: {extensionDuration} hour(s).";
        }

        public void displayExtensionConfirmation()
        {
            rentalDetailsView = "Rental extension confirmed.";
        }

        public void displayExtensionUnavailableMessage()
        {
            rentalDetailsView = "No alternative account is available.";
        }

        public void displayExtensionRejectedMessage()
        {
            rentalDetailsView = "The extension request was rejected.";
        }

        public void extensionConfirmed()
        {
            rentalDetailsView = "Extension successful.";
        }

        public void extensionRejected()
        {
            rentalDetailsView = "Extension rejected.";
        }

        public void selectRentalDuration(int duration)
        {
            rentalDetailsView = $"Selected duration: {duration} hour(s).";
        }
    }
}
