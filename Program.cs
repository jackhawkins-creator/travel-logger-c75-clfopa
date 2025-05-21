using TravelLogger.Models;
using TravelLogger.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<TravelLoggerDbContext>(builder.Configuration["TravelLoggerDbConnectionString"]);

var app = builder.Build();

// Comment out HTTPS redirection for now to simplify testing
// app.UseHttpsRedirection();

// Configure Swagger for all environments during development
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});

// Add all endpoints here

//LOGS
//GET all logs for a given user
app.MapGet("/api/users/{userId}/logs", (TravelLoggerDbContext db, int userId) =>
{
    User user = db.Users
        .Include(u => u.Logs)
        .SingleOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return Results.NotFound();
    }

    List<LogDTO> userLogs = user.Logs
        .Select(log => new LogDTO
        {
            Id = log.Id,
            UserId = log.UserId,
            CityId = log.CityId,
            Comment = log.Comment,
            CreatedAt = log.CreatedAt
        }).ToList();

    UserDTO userDTO = new UserDTO
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        ImageUrl = user.ImageUrl,
        Description = user.Description,
        Logs = userLogs
    };

    return Results.Ok(userDTO);
});


//RECS
//GET All Recs (not needed, but good for testing purposes)
app.MapGet("/api/recommendations", (TravelLoggerDbContext db) =>
{
    return db.Recommendations
    .Select(r => new RecommendationDTO
    {
        Id = r.Id,
        UserId = r.UserId,
        CityId = r.CityId
    }).ToList();
});

//POST New Rec
app.MapPost("/api/recommendations", (TravelLoggerDbContext db, Recommendation recommendation) =>
{
    db.Recommendations.Add(recommendation);
    db.SaveChanges();
    return Results.Created($"/api/recommendations/{recommendation.Id}", recommendation);
});

/*
Input below as raw data when POSTing to test...
{
  "userId": 1,
  "cityId": 2
}
*/

//PUT Update Rec
app.MapPut("/api/recommendations/{id}", (TravelLoggerDbContext db, int id, Recommendation recommendation) =>
{
    Recommendation recommendationToUpdate = db.Recommendations.SingleOrDefault(recommendation => recommendation.Id == id);
    if (recommendationToUpdate == null)
    {
        return Results.NotFound();
    }
    recommendationToUpdate.UserId = recommendation.UserId;
    recommendationToUpdate.CityId = recommendation.CityId;

    db.SaveChanges();
    return Results.NoContent();
});

/*
Input below as raw data when PUTting to test...
{
  "userId": 2,
  "cityId": 1
}
*/

//DELETE Rec
app.MapDelete("/api/recommendations/{id}", (TravelLoggerDbContext db, int id) =>
{
    Recommendation recommendation = db.Recommendations.SingleOrDefault(recommendation => recommendation.Id == id);
    if (recommendation == null)
    {
        return Results.NotFound();
    }
    db.Recommendations.Remove(recommendation);
    db.SaveChanges();
    return Results.NoContent();
});

//GET all recs for a given city
app.MapGet("/api/cities/{cityId}/recommendations", async (TravelLoggerDbContext db, int cityId) =>
{
    // Fetch the city and its recommendations
    City city = await db.Cities
        .Where(c => c.Id == cityId)
        .Include(c => c.Recommendations)
        .FirstOrDefaultAsync();

    if (city == null)
    {
        return Results.NotFound();
    }

    // Return the city and its recommendations
    return Results.Ok(new CityDTO
    {
        Id = city.Id,
        Name = city.Name,
        Details = city.Details,
        Recommendations = city.Recommendations.Select(r => new RecommendationDTO
        {
            Id = r.Id,
            UserId = r.UserId,
            CityId = r.CityId
        }).ToList()
    });
});

//GET all upvotes for a rec
app.MapGet("/api/recommendations/{id}", (TravelLoggerDbContext db, int id) =>
{
    Recommendation recommendation = db.Recommendations.SingleOrDefault(r => r.Id == id);

    if (recommendation == null)
    {
        return Results.NotFound();
    }

    int upvoteCount = db.Upvotes.Count(u => u.RecommendationId == id);

    RecommendationDTO recommendationDTO = new RecommendationDTO
    {
        Id = recommendation.Id,
        UserId = recommendation.UserId,
        CityId = recommendation.CityId,
        UpvoteTotal = upvoteCount
    };

    return Results.Ok(recommendationDTO);
});


//UPVOTE ENDPOINTS
//POST upvote to recommendation
app.MapPost("/api/upvotes", (TravelLoggerDbContext db, Upvote upvote) =>
{
    // Validate the recommendation exists
    Recommendation recommendation = db.Recommendations.SingleOrDefault(r => r.Id == upvote.RecommendationId);

    if (recommendation == null)
    {
        return Results.BadRequest($"Recommendation with ID {upvote.RecommendationId} does not exist.");
    }

    // Add the upvote
    db.Upvotes.Add(upvote);
    db.SaveChanges();

    return Results.Created($"/api/upvotes/{upvote.Id}", upvote);
});

/*
Input below as raw data when POSTing to test...
{
  "recommendationId": 1
}

...Use /api/recommendations/1 endpoint to check if upvote total incremented
*/

//DELETE upvote for a recommendation
app.MapDelete("/api/upvotes/{id}", (TravelLoggerDbContext db, int id) =>
{
    Upvote upvoteToDelete = db.Upvotes.SingleOrDefault(upvote => upvote.Id == id);
    if (upvoteToDelete == null)
    {
        return Results.NotFound();
    }

    db.Upvotes.Remove(upvoteToDelete);
    db.SaveChanges();
    return Results.NoContent();
});

// Cities endpoints
// get cities
app.MapGet("/api/cities", (TravelLoggerDbContext db) =>
{
    return db.Cities
        .Select(c => new CityDTO
        {
            Id = c.Id,
            Name = c.Name,
            Details = c.Details
        }).ToList();
});

// Get cities details with logs, users there, and recommendations 
app.MapGet("/api/cities/{id}", (TravelLoggerDbContext db, int id) =>
{
    City foundCity = db.Cities
        .Include(c => c.Logs)
            .ThenInclude(l => l.User)
        .Include(c => c.Recommendations)
        .FirstOrDefault(c => c.Id == id);


    if (foundCity == null)
    {
        return Results.NotFound();
    }

    List<RecommendationDTO> recommendationDTOs = foundCity.Recommendations
        .Select(r => new RecommendationDTO
        {
            Id = r.Id,
            UserId = r.UserId,
            CityId = r.CityId,
            UpvoteTotal = db.Upvotes.Count(u => u.RecommendationId == r.Id)
        })
        .ToList();

    List<LogDTO> logDTOs = foundCity.Logs
        .Select(log => new LogDTO
        {
            Id = log.Id,
            UserId = log.UserId,
            CityId = log.CityId,
            Comment = log.Comment,
            CreatedAt = log.CreatedAt
        })
        .ToList();

    List<UserDTO> userDTOs = foundCity.Logs
        .Select(log => new UserDTO
        {
            Id = log.User.Id,
            Email = log.User.Email,
            Description = log.User.Description,
            ImageUrl = log.User.ImageUrl,
            Name = log.User.Name
        })
        .DistinctBy(u => u.Id)
        .ToList();

    CityDTO cityDTO = new CityDTO
    {
        Id = foundCity.Id,
        Name = foundCity.Name,
        Details = foundCity.Details,
        Recommendations = recommendationDTOs,
        Logs = logDTOs,
        Users = userDTOs
    };

    return Results.Ok(cityDTO);
});


app.Run();