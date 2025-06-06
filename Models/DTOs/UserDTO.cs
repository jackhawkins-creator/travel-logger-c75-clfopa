namespace TravelLogger.Models.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<LogDTO> Logs { get; set; }
        public List<Recommendation> Recommendations { get; set; }
    }
}
