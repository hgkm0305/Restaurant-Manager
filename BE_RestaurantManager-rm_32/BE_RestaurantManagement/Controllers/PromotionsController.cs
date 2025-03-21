using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }


        // GET: Get All Promotions (Admin/Staff)
        [HttpGet("get-all-promotions")]
        [Authorize(Roles = "2,4")]
        public async Task<IActionResult> GetAllPromotions()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        // POST: Create Promotion (Admin only)
        [HttpPost("create-promotion")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Create([FromBody] PromotionDTO promotionDTO)
        {
            var promotionId = await _promotionService.CreatePromotionAsync(promotionDTO);
            return CreatedAtAction(nameof(GetPromotionById), new { id = promotionId }, promotionDTO);
        }

        // PUT: Update Promotion (Admin only)
        [HttpPut("update-promotion/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Update(int id, [FromBody] PromotionDTO promotionDTO)
        {
            await _promotionService.UpdatePromotionAsync(id, promotionDTO);
            var updatedPromotion = await _promotionService.GetPromotionByIdAsync(id);

            return Ok(updatedPromotion);
        }

        // DELETE: Delete Promotion (Admin only)
        [HttpDelete("delete-promotion/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeletePromotionAsync(id);
            return Ok(new { message = "Promotion deleted successfully" });
        }

        // GET: Get Promotion Detail (Admin/Staff)
        [HttpGet("get-detail-promotion/{id}")]
        [Authorize(Roles = "2,4")]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        // POST: Apply Promotion to Order (Admin/Staff)
        [HttpPost("apply-promotion")]
        [Authorize(Roles = "2,4")]
        public async Task<IActionResult> Apply([FromBody] PromotionApplyDTO dto)   
        {
            await _promotionService.ApplyPromotionAsync(dto);
            return Ok("Promotion applied successfully");
        }

        // POST: Cancel Promotion (Admin only)
        [HttpPost("cancel-promotion/{promotionId}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Cancel(int promotionId)
        {
            await _promotionService.CancelPromotionAsync(promotionId);
            return Ok("Promotion canceled successfully");
        }
    }
}