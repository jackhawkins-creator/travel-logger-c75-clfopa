using Microsoft.EntityFrameworkCore;
using TravelLogger.Models;

public class TravelLoggerDbContext : DbContext
{
    // Define tables here
    public DbSet<User> Users { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<Upvote> Upvotes { get; set; }

    public TravelLoggerDbContext(DbContextOptions<TravelLoggerDbContext> context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed Users
        modelBuilder.Entity<User>().HasData(new User[]
        {
            new User
            {
                Id = 1,
                Name = "Jamie Chen",
                Email = "jamie.chen@example.com",
                ImageUrl = "https://example.com/images/jamie.jpg",
                Description = "World traveler and foodie."
            },
            new User
            {
                Id = 2,
                Name = "Carlos Diaz",
                Email = "carlos.diaz@example.com",
                ImageUrl = "https://example.com/images/carlos.jpg",
                Description = "Adventure seeker and nature lover."
            },
            new User
            {
                Id = 3,
                Name = "Ava Singh",
                Email = "ava.singh@example.com",
                ImageUrl = "https://example.com/images/ava.jpg",
                Description = "Photographer capturing the world's beauty."
            },
            new User
            {
                Id = 4,
                Name = "Liam Patel",
                Email = "liam.patel@example.com",
                ImageUrl = "https://example.com/images/liam.jpg",
                Description = "Explorer of hidden gems and local cuisines."
            }
        });

        // Seed Cities
        modelBuilder.Entity<City>().HasData(new City[]
        {
            new City
            {
                Id = 1,
                Name = "Tokyo",
                Details = "A bustling metropolis blending tradition and technology."
            },
            new City
            {
                Id = 2,
                Name = "Barcelona",
                Details = "Known for its art, architecture, and vibrant street life."
            },
            new City
            {
                Id = 3,
                Name = "Cape Town",
                Details = "A city with scenic beauty and diverse culture."
            },
            new City
            {
                Id = 4,
                Name = "Vancouver",
                Details = "Surrounded by mountains and ocean, perfect for outdoor lovers."
            }
        });

        // Seed Logs
        modelBuilder.Entity<Log>().HasData(new Log[]
        {
            new Log
            {
                Id = 1,
                UserId = 1,
                CityId = 1,
                Comment = "Tokyo was incredible! Loved the food and the culture.",
                CreatedAt = new DateTime(2025, 4, 10)
            },
            new Log
            {
                Id = 2,
                UserId = 2,
                CityId = 2,
                Comment = "Barcelona's Gothic Quarter is a must-see!",
                CreatedAt = new DateTime(2025, 4, 15)
            },
            new Log
            {
                Id = 3,
                UserId = 3,
                CityId = 3,
                Comment = "Cape Town's Table Mountain view is breathtaking!",
                CreatedAt = new DateTime(2025, 5, 1)
            },
            new Log
            {
                Id = 4,
                UserId = 4,
                CityId = 4,
                Comment = "Vancouver's seafood and hiking trails are unbeatable.",
                CreatedAt = new DateTime(2025, 5, 3)
            },
            new Log
            {
                Id = 5,
                UserId = 1,
                CityId = 3,
                Comment = "I loved the cultural diversity of Cape Town.",
                CreatedAt = new DateTime(2025, 5, 7)
            }
        });

        // Seed Recommendations
        modelBuilder.Entity<Recommendation>().HasData(new Recommendation[]
        {
            new Recommendation
            {
                Id = 1,
                UserId = 1,
                CityId = 2
            },
            new Recommendation
            {
                Id = 2,
                UserId = 2,
                CityId = 1
            },
            new Recommendation
            {
                Id = 3,
                UserId = 3,
                CityId = 4
            },
            new Recommendation
            {
                Id = 4,
                UserId = 4,
                CityId = 3
            },
            new Recommendation
            {
                Id = 5,
                UserId = 2,
                CityId = 4
            }
        });

        // Seed Upvotes
        modelBuilder.Entity<Upvote>().HasData(new Upvote[]
        {
            new Upvote
            {
                Id = 1,
                RecommendationId = 1
            },
            new Upvote
            {
                Id = 2,
                RecommendationId = 2
            },
            new Upvote
            {
                Id = 3,
                RecommendationId = 3
            },
            new Upvote
            {
                Id = 4,
                RecommendationId = 1
            },
            new Upvote
            {
                Id = 5,
                RecommendationId = 5
            }
        });
    }
}
