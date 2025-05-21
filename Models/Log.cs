using System.ComponentModel.DataAnnotations;


namespace TravelLogger.Models;


public class Log
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CityId { get; set; }
    [Required]
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public City City { get; set; }
}
