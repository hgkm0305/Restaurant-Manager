using System.ComponentModel.DataAnnotations.Schema;

namespace BE_RestaurantManagement.Models
{
    [Table("KitchenStaffs")]
    public class KitchenStaff : User
    {
        [InverseProperty("KitchenStaff")]
        public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();
    }
}
