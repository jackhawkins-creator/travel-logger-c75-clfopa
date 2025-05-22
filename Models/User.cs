using System.ComponentModel.DataAnnotations;


namespace TravelLogger.Models;


public class User
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string ImageUrl { get; set; }
    [Required]
    public string Description { get; set; }
    public List<Log> Logs { get; set; }
    public List<Recommendation> Recommendations { get; set; }
}
