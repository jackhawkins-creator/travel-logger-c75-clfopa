namespace TravelLogger.Models;


public class Recommendation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CityId { get; set; }
    public int UpvoteCount { get; set; }

    public User User { get; set; }
    public City City { get; set; }
    public List<Upvote> Upvotes { get; set; }
}
