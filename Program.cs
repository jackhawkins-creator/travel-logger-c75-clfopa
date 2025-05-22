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

app.MapPost("/api/users", (TravelLoggerDbContext db, User user) =>
{
    db.Users.Add(user);
    db.SaveChanges();
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapGet("/api/users/signin/{email}", (TravelLoggerDbContext db, string email) =>
{
    User user = db.Users.SingleOrDefault(u => u.Email == email);

    if (user == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(user);

});

app.MapGet("/api/users/{id}", async (TravelLoggerDbContext db, int id) =>
{
    User user = await db.Users
            .Where(u => u.Id == id)
            .Include(u => u.Logs)
            .Include(u => u.Recommendations)
            .FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound();
    }

    UserDTO userDTO = new UserDTO
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        ImageUrl = user.ImageUrl,
        Description = user.Description,
        Logs = user.Logs,
        Recommendations = user.Recommendations
    };

    return Results.Ok(userDTO);
});

app.MapPut("/api/users/{id}", (TravelLoggerDbContext db, int id, UserDTO userDTO) =>
{
    User userToUpdate = db.Users.SingleOrDefault(user => user.Id == id);
    if (userToUpdate == null)
    {
        return Results.NotFound();
    }

    userToUpdate.Name = userDTO.Name;
    userToUpdate.Email = userDTO.Email;
    userToUpdate.ImageUrl = userDTO.ImageUrl;
    userToUpdate.Description = userDTO.Description;

    db.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/cities/{cityId}/users", async (TravelLoggerDbContext db, int cityId) =>
{
    var usersInCity = await db.Users.ToListAsync();

    usersInCity = usersInCity.Where(user =>
    {
        var mostRecentLog = db.Logs
            .Where(log => log.UserId == user.Id)
            .OrderByDescending(log => log.CreatedAt)
            .FirstOrDefault();

        return mostRecentLog != null && mostRecentLog.CityId == cityId;
    }).ToList();

    var userDTOs = usersInCity.Select(user => new UserDTO
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        ImageUrl = user.ImageUrl,
        Description = user.Description
    }).ToList();

    return Results.Ok(userDTOs);
});

app.Run();