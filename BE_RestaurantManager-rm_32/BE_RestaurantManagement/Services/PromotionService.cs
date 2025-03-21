using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Models;
using BE_RestaurantManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;
using BE_RestaurantManagement.Interfaces;

namespace BE_RestaurantManagement.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly RestaurantDbContext _context;

        public PromotionService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            return await _context.Promotions
                .Include(p => p.Orders)
                .ToListAsync();
        }

        public async Task<int> CreatePromotionAsync(PromotionDTO promotionDTO)
        {
            var promotion = new Promotion
            {
                Name = promotionDTO.Name,
                Description = promotionDTO.Description,
                DiscountPercentage = promotionDTO.DiscountPercentage,
                StartDate = promotionDTO.StartDate,
                EndDate = promotionDTO.EndDate,
                IsActive = promotionDTO.IsActive
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion.PromotionId;
        }

        public async Task UpdatePromotionAsync(int id, PromotionDTO promotionDTO)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) throw new Exception("Promotion not found.");

            promotion.Name = promotionDTO.Name;
            promotion.Description = promotionDTO.Description;
            promotion.DiscountPercentage = promotionDTO.DiscountPercentage;
            promotion.StartDate = promotionDTO.StartDate;
            promotion.EndDate = promotionDTO.EndDate;
            promotion.IsActive = promotionDTO.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) throw new Exception("Promotion not found.");

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }

        public async Task<Promotion> GetPromotionByIdAsync(int id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p => p.PromotionId == id);

            if (promotion == null)
            {
                throw new Exception("Promotion not found.");
            }

            return promotion;
        }

        public async Task ApplyPromotionAsync(PromotionApplyDTO dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) throw new Exception("Order not found.");

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromotionId == dto.PromotionId);

            if (promotion == null || !promotion.IsActive ||
                promotion.EndDate < DateTime.Now || promotion.StartDate > DateTime.Now)
                throw new Exception("Promotion is invalid or expired.");

            order.PromotionId = promotion.PromotionId;
            await _context.SaveChangesAsync();
        }

        public async Task CancelPromotionAsync(int promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) throw new Exception("Khuyến mãi không tồn tại.");

            promotion.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}