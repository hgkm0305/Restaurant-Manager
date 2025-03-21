using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE_RestaurantManagement.Interfaces
{
    public interface IPromotionService
    {
        Task<int> CreatePromotionAsync(PromotionDTO promotionDTO);
        Task<List<Promotion>> GetAllPromotionsAsync();
        Task UpdatePromotionAsync(int id, PromotionDTO promotionDTO);
        Task DeletePromotionAsync(int id);
        Task<Promotion> GetPromotionByIdAsync(int id);
        Task ApplyPromotionAsync(PromotionApplyDTO dto);
        Task CancelPromotionAsync(int promotionId);
    }
}