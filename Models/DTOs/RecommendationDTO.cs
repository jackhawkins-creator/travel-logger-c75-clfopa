namespace TravelLogger.Models.DTOs
{
    public class RecommendationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CityId { get; set; }
        public int UpvoteTotal { get; set; }
    }
}
