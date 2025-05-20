using System.ComponentModel.DataAnnotations;

namespace TravelLogger.Models;

public class City
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Details { get; set; }
}