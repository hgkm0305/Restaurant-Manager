using Azure.Core;
using BE_RestaurantManagement.Data;
using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Interfaces;
using BE_RestaurantManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestaurantAPI.Models;
using System.Security.Claims;

namespace BE_RestaurantManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly RestaurantDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public OrderService(RestaurantDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        //DONE
        public async Task<Order> CreateOrderAsync(OrderCreateRequest request)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            var kitchenStaff = await _context.KitchenStaffs.FindAsync(request.KitchenStaffId);
            if (kitchenStaff == null)
            {
                throw new Exception("KitchenStaff not found");
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid token: User ID not found");
            }

            int userId = int.Parse(userIdClaim.Value);

            var Staff = await _context.Staffs.FindAsync(userId);
            if (Staff == null)
            {
                throw new Exception("Staff not found");
            }

            // Get desk information but not update the state immediately
            Models.Table? table = null;
            if (request.TableId != null)
            {
                table = await _context.Tables.FirstOrDefaultAsync(t => t.TableId == request.TableId)
                    ?? throw new KeyNotFoundException("Table not found");

                if (table.Status == "Reserved")
                {
                    throw new Exception("Table is already reserved");
                }
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>(),
                StaffId = userId,
                Staff = Staff,
                KitchenStaffId = request.KitchenStaffId,
                KitchenStaff = kitchenStaff,
                TableId = request.TableId,
                Table = table
            };

            foreach (var item in request.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable)
                {
                    throw new Exception($"Menu item {item.MenuItemId} is not available");
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    TotalPrice = menuItem.Price * item.Quantity
                };

                order.OrderItems.Add(orderItem);
            }

            // **Pack the entire logic to save orders in try ... Catch **
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Only commit orders, not updated table

                // If there is no fault, then update the table status
                if (table != null)
                {
                    table.Status = "Reserved";
                    _context.Tables.Update(table);
                    await _context.SaveChangesAsync();
                }

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create order", ex);
            }
        }

        //DONE
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            try
            {
                // Check if the order has a goal and a table is in a reserved state
                if (order.TableId != null)
                {
                    var table = await _context.Tables.FindAsync(order.TableId);
                    if (table != null && table.Status == "Reserved")
                    {
                        table.Status = "Available"; // Reset table status
                        _context.Tables.Update(table);
                    }
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync(); // Save changes at the same time to ensure consistency

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete order", ex);
            }
        }

        //DONE
        public async Task<IEnumerable<OrderGetAllRequest>> GetAllOrdersAsync()
        {

            return await _context.Orders
                 .Select(o => new OrderGetAllRequest
                 {
                     OrderId = o.OrderId,
                     CustomerId = o.CustomerId,
                     CustomerName = o.Customer.FullName,
                     StaffId = o.Staff.UserId,
                     StaffName = o.Staff.FullName,
                     KitchenStaffId = o.KitchenStaffId,
                     KitchenStaffName = o.KitchenStaff.FullName,
                     OrderDate = o.OrderDate,
                     OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                     {
                         OrderItemId = oi.OrderItemId,
                         MenuItemId = oi.MenuItemId,
                         MenuItemName = oi.MenuItem.Name,
                         Price = oi.MenuItem.Price,
                         Quantity = oi.Quantity,
                         TotalPrice = oi.TotalPrice
                     }).ToList()
                 })
                 .ToListAsync();
        }

        //DONE
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking() // There is no need for tracking if you only read data
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.KitchenStaff)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            return order;
        }

        //DONE
        public async Task<Order> UpdateOrderAsync(int orderId, OrderUpdateRequest request)
        {
            // Check order
            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            // Check Kitchen Staff
            KitchenStaff? kitchenStaffData;
            var kitchenStaffExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == request.KitchenStaffId && u.RoleId == 6);
            if (!kitchenStaffExists)
            {
                throw new ArgumentException("Invalid Kitchen Staff ID.");
            } else
            {
                kitchenStaffData = await _context.KitchenStaffs.FindAsync(request.KitchenStaffId);
            }

            // Check Staff
            Staff? staffData;
            var staffExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == request.StaffId && u.RoleId == 4);
            if (!staffExists)
            {
                throw new ArgumentException("Invalid Staff ID.");
            }
            else
            {
                staffData = await _context.Staffs.FindAsync(request.StaffId);
            }

            // Check Customer
            Customer? customerData;
            var customerExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == request.CustomerId && u.RoleId == 5);
            if (!customerExists)
            {
                throw new ArgumentException("Invalid Customer ID.");
            }else
            {
                customerData = await _context.Customers.FindAsync(request.CustomerId);
            }

            // Update infor order
            existingOrder.CustomerId = request.CustomerId;
            existingOrder.Customer = customerData;
            existingOrder.StaffId = request.StaffId;
            existingOrder.Staff = staffData;
            existingOrder.KitchenStaffId = request.KitchenStaffId;
            existingOrder.KitchenStaff = kitchenStaffData;
            existingOrder.Status = request.Status;
            existingOrder.OrderDate = request.OrderDate;

            // Update OrderItems (xóa cũ, thêm mới)
            existingOrder.OrderItems.Clear();
            foreach (var item in request.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable)
                {
                    throw new Exception($"Menu item {item.MenuItemId} is not available");
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    TotalPrice = menuItem.Price * item.Quantity
                };

                existingOrder.OrderItems.Add(orderItem);
            }

            // save to database
            await _context.SaveChangesAsync();
            return existingOrder;
        }

    }
}
