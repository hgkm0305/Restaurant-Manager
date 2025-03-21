using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BE_RestaurantManagement.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            try
            {
                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(request.Amount, request.Currency);
                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("confirm-payment-intent")]
        public async Task<IActionResult> ConfirmPaymentIntent([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var paymentIntent = await _paymentService.ConfirmPaymentIntentAsync(request.PaymentIntentId, request.PaymentMethodId);
                return Ok(new { status = paymentIntent.Status });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentDataRequest request)
        {
            try
            {
                var paymentResponse = await _paymentService.ProcessPaymentAsync(request);
                return Ok(paymentResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var paymentIntent = await _paymentService.ConfirmPaymentIntentAsync(request.PaymentIntentId, request.PaymentMethodId);
                return Ok(new { status = paymentIntent.Status });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class PaymentRequest
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
