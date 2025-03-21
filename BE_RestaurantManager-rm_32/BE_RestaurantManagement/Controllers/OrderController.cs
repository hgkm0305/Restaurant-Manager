﻿using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Interfaces;
using BE_RestaurantManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_RestaurantManagement.Controllers
{
    [Authorize(Roles = "2,3,4")] // Admin/Manager/Staff
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request);
                return Ok(new { message = "Order created successfully", order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-order-by-id/{orderId}")]
        public async Task<ActionResult<Order>> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        [HttpDelete("delete-order/{orderId}")]
        public async Task<ActionResult> DeleteOrder(int orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            if (!result) return NotFound("Order not found.");
            return Ok(new { message = "Order is deleted successfully" });
        }

        [HttpPut("update-order/{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] OrderUpdateRequest request)
        {
            try
            {
                if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            var updatedOrder = await _orderService.UpdateOrderAsync(orderId, request);

            if (updatedOrder == null)
            {
                return NotFound(new { Message = "Order not found." });
            }

            return Ok(updatedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Database update error: " + ex.InnerException?.Message }); // 400 nếu lỗi database
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message }); // 500 nếu lỗi không xác định
            }
        }

        [HttpGet("get-all-orders")]
        public async Task<ActionResult<IEnumerable<OrderGetAllRequest>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

    }
}
