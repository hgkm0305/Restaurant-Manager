namespace BE_RestaurantManagement.DTOs
{
    public class PaymentDataRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } // "Cash" or "Card"
        public string? CardToken { get; set; } // Chỉ dùng khi thanh toán bằng thẻ
    }
}
