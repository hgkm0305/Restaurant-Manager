using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_RestaurantManagement.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string PaymentMethod { get; set; } // Cash, Stripe Card

        [Column(TypeName = "nvarchar(20)")]
        public string Status { get; set; } // "Pending", "Completed", "Failed", "Refunded"

        [Column(TypeName = "nvarchar(100)")]
        public string TransactionId { get; set; } // ID từ Stripe, PayPal, VNPay, v.v.

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "nvarchar(255)")]
        public string? ReceiptUrl { get; set; } // URL hóa đơn từ Stripe (nếu có)

    }
}
