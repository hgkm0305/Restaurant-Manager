using BE_RestaurantManagement.Controllers;
using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Models;
using Stripe;

namespace BE_RestaurantManagement.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency);
        Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId);
        Task<PaymentResponse> ProcessPaymentAsync(PaymentDataRequest request);

        //Task<Payment> ConfirmStripePaymentAsync(string transactionId);
    }
}
