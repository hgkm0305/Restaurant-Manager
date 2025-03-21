namespace BE_RestaurantManagement.DTOs
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string ReceiptUrl { get; set; }
        public string ClientSecret { get; set; } // Chỉ có khi thanh toán bằng Card

    }
}
