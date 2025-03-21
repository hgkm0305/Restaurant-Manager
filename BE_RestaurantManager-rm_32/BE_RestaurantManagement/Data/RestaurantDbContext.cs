using BE_RestaurantManagement.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace BE_RestaurantManagement.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

        // Định nghĩa DbSet cho từng entity (tương ứng với các bảng trong database)
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<KitchenStaff> KitchenStaffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<RevenueReport> RevenueReports { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User - Role (1-N)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Customer - Order (1-N)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            // Staff - Order (1-N) (Nhân viên phục vụ xử lý nhiều đơn hàng)
            modelBuilder.Entity<Staff>()
                .HasMany(s => s.ProcessedOrders)
                .WithOne(o => o.Staff)
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho phép xóa Staff nếu còn Order

            modelBuilder.Entity<KitchenStaff>()
                .HasMany(ks => ks.AssignedOrders)
                .WithOne(o => o.KitchenStaff)
                .HasForeignKey(o => o.KitchenStaffId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho phép xóa KitchenStaff nếu còn Order

            // Order - OrderItem (1-N)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // MenuItem - OrderItem (1-N)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId);

            // Order - Promotion (N-1)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Promotion validation (Discount không được quá 100%)
            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountPercentage)
                .HasPrecision(5, 2);

            // Table - Order (1-1) (Mỗi bàn có thể có 1 đơn hàng)
            modelBuilder.Entity<Table>()
                .HasOne(t => t.Order)
                .WithOne(o => o.Table)
                .HasForeignKey<Table>(t => t.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
