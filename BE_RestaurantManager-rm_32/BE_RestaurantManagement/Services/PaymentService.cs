using BE_RestaurantManagement.Controllers;
using BE_RestaurantManagement.Data;
using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Interfaces;
using BE_RestaurantManagement.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BE_RestaurantManagement.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeClient _stripeClient;
        private readonly RestaurantDbContext _context;


        public PaymentService(IConfiguration configuration, RestaurantDbContext context)
        {
            _stripeClient = new StripeClient(configuration["Stripe:SecretKey"]);
            _context = context;

        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount, // Số tiền tính bằng cent (ví dụ: 1000 = 10 USD)
                Currency = currency, // Đơn vị tiền tệ (usd, vnd,...)
                PaymentMethodTypes = new List<string> { "card" }
            };

            var service = new PaymentIntentService(_stripeClient);
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId)
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            };

            var service = new PaymentIntentService(_stripeClient);
            return await service.ConfirmAsync(paymentIntentId, options);
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentDataRequest request)
        {
            // 1️⃣ Lấy Order từ DB
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Promotion) // Lấy thông tin khuyến mãi
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // 2️⃣ Tính tổng tiền của order
            decimal totalAmount = order.OrderItems.Sum(oi => oi.MenuItem.Price * oi.Quantity);

            // 3️⃣ Kiểm tra và áp dụng khuyến mãi nếu có
            if (order.Promotion != null && order.Promotion.IsActive)
            {
                bool isPromotionValid = order.Promotion.StartDate <= DateTime.UtcNow &&
                                        order.Promotion.EndDate >= DateTime.UtcNow;

                if (isPromotionValid)
                {
                    decimal discount = totalAmount * (order.Promotion.DiscountPercentage / 100);
                    totalAmount -= discount;
                }
            }

            // 4️⃣ Xử lý thanh toán
            string transactionId = null;
            string receiptUrl = null;
            string paymentStatus = "Pending"; // Mặc định là Pending
            string clientSecret = null;

            if (request.PaymentMethod == "Card")
            {
                // Gọi Stripe API để tạo PaymentIntent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(totalAmount * 100), // Đơn vị cents
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var service = new PaymentIntentService(_stripeClient);
                var paymentIntent = await service.CreateAsync(options);

                // Trả về ClientSecret để client xử lý tiếp
                clientSecret = paymentIntent.ClientSecret;
                transactionId = paymentIntent.Id;
            }
            else
            {
                // Nếu thanh toán bằng tiền mặt, tạo Payment ngay
                paymentStatus = "Completed";
            }

            // 5️⃣ Tạo bản ghi Payment
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = paymentStatus,
                TransactionId = transactionId,
                ReceiptUrl = receiptUrl,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 6️⃣ Trả về thông tin thanh toán
            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                ReceiptUrl = payment.ReceiptUrl,
                ClientSecret = clientSecret // Trả về cho client xử lý nếu dùng Card
            };
        }
    }
}
