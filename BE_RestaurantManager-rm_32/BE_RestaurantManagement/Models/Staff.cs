using System.ComponentModel.DataAnnotations.Schema;

namespace BE_RestaurantManagement.Models
{
    [Table("Staffs")]
    public class Staff : User
    {
        [InverseProperty("Staff")]
        public ICollection<Order> ProcessedOrders { get; set; } = new List<Order>();
        /*
        [InverseProperty("Staff")]

        Annotation này giúp EF hiểu rằng quan hệ giữa Order và Staff được ánh xạ qua thuộc tính Staff 
        bên trong class Order.
        Nếu không có [InverseProperty], EF có thể bị nhầm lẫn khi ánh xạ quan hệ 1-N với Order. 

        Lý do bảng Staffs không lưu thông tin Order trực tiếp là vì:
        Quan hệ đã được thiết lập ngược lại:
        Staff có danh sách ProcessedOrders, nhưng khóa ngoại StaffId lại nằm trong bảng Orders, 
        không phải Staffs.
        Điều này có nghĩa là Orders sẽ chứa thông tin về nhân viên nào xử lý nó, chứ không phải ngược lại.
        */
    }
}
