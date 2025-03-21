using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_RestaurantManagement.Models
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }

        public int Capacity { get; set; }

        public int? OrderId { get; set; } // Cho phép null để bàn có thể trống
        [ForeignKey("OrderId")]
        public Order Order { get; set; } // Một Table có một Order

        [Column(TypeName = "nvarchar(50)")]
        public string Status { get; set; } // Available, Reserved
    }
}
