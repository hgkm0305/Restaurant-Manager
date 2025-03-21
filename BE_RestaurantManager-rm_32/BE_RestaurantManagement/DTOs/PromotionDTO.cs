namespace BE_RestaurantManagement.DTOs
{
    public class PromotionDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PromotionApplyDTO
    {
        public int OrderId { get; set; }
        public int PromotionId { get; set; }
    }
}