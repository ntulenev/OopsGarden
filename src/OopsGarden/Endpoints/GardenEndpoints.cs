using Microsoft.EntityFrameworkCore;

using Models;
using Storage;
using Transport;

namespace OopsGarden.Endpoints;

internal static class GardenEndpoints
{
    public static void MapGardenEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/garden").RequireAuthorization();

        group.MapGet("/summary", async (GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var plants = await db.Plants
                .Where(plant => plant.UserId == userId)
                .Include(plant => plant.Location)
                .Select(plant => new
                {
                    Id = plant.Id.Value,
                    Name = plant.Name.Value,
                    Description = plant.Description.Value,
                    PhotoDataUrl = plant.PhotoDataUrl == null ? null : plant.PhotoDataUrl.Value.Value,
                    plant.PlantedOn,
                    Location = plant.Location == null
                        ? null
                        : new { Id = plant.Location.Id.Value, Name = plant.Location.Name.Value },
                    LastWateredAt = plant.WateringEvents
                        .OrderByDescending(watering => watering.WateredAt)
                        .Select(watering => (DateTimeOffset?)watering.WateredAt)
                        .FirstOrDefault()
                })
                .OrderBy(plant => plant.Name)
                .ToListAsync();

            return Results.Ok(plants);
        });

        group.MapPost("/plants/{id:guid}/water", async (Guid id, GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var plantId = PlantId.From(id);
            var plant = await db.Plants.SingleOrDefaultAsync(plant => plant.Id == plantId && plant.UserId == userId);
            if (plant is null)
            {
                return Results.NotFound();
            }

            var watering = plant.Water();
            _ = db.WateringEvents.Add(watering);
            await db.SaveChangesAsync();
            return Results.Ok(new { watering.WateredAt });
        });

        group.MapGet("/locations", async (GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            return await db.Locations
                .Where(location => location.UserId == userId)
                .OrderBy(location => location.Name)
                .Select(location => new
                {
                    Id = location.Id.Value,
                    Name = location.Name.Value,
                    Plants = location.Plants.Count
                })
                .ToListAsync();
        });

        group.MapPost("/locations", async (LocationRequest request, GardenDbContext db, HttpContext http) =>
        {
            var location = Location.Create(http.User.CurrentUserId(), LocationName.From(request.Name));
            _ = db.Locations.Add(location);
            await db.SaveChangesAsync();
            return Results.Ok(new { Id = location.Id.Value, Name = location.Name.Value });
        });

        group.MapDelete("/locations/{id:guid}", async (Guid id, GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var locationId = LocationId.From(id);
            var location = await db.Locations.SingleOrDefaultAsync(location => location.Id == locationId && location.UserId == userId);
            if (location is null)
            {
                return Results.NotFound();
            }

            _ = db.Locations.Remove(location);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapGet("/plants", async (GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            return await db.Plants
                .Where(plant => plant.UserId == userId)
                .Include(plant => plant.Location)
                .OrderBy(plant => plant.Name)
                .Select(plant => new
                {
                    Id = plant.Id.Value,
                    Name = plant.Name.Value,
                    Description = plant.Description.Value,
                    PhotoDataUrl = plant.PhotoDataUrl.HasValue
                        ? plant.PhotoDataUrl.Value.Value
                        : null,
                    plant.PlantedOn,
                    LocationId = plant.LocationId.HasValue
                        ? plant.LocationId.Value.Value
                        : (Guid?)null,
                    LocationName = plant.Location != null ? plant.Location.Name.Value : null
                })
                .ToListAsync();
        });

        group.MapPost("/plants", async (PlantRequest request, GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var requestLocationId = request.LocationId.HasValue
                ? LocationId.From(request.LocationId.Value)
                : (LocationId?)null;
            if (requestLocationId is not null && !await db.Locations.AnyAsync(location => location.Id == requestLocationId && location.UserId == userId))
            {
                return Results.BadRequest(new { error = "Invalid location." });
            }

            var plant = Plant.Create(
                userId,
                PlantName.From(request.Name),
                PlantDescription.From(request.Description),
                requestLocationId,
                request.PlantedOn,
                request.PhotoDataUrl);
            _ = db.Plants.Add(plant);
            await db.SaveChangesAsync();
            return Results.Ok(new { Id = plant.Id.Value });
        });

        group.MapPut("/plants/{id:guid}", async (Guid id, PlantRequest request, GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var plantId = PlantId.From(id);
            var plant = await db.Plants.SingleOrDefaultAsync(plant => plant.Id == plantId && plant.UserId == userId);
            if (plant is null)
            {
                return Results.NotFound();
            }

            var requestLocationId = request.LocationId.HasValue
                ? LocationId.From(request.LocationId.Value)
                : (LocationId?)null;
            if (requestLocationId is not null && !await db.Locations.AnyAsync(location => location.Id == requestLocationId && location.UserId == userId))
            {
                return Results.BadRequest(new { error = "Invalid location." });
            }

            plant.UpdateDetails(
                PlantName.From(request.Name),
                PlantDescription.From(request.Description),
                requestLocationId,
                request.PlantedOn,
                request.PhotoDataUrl);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/plants/{id:guid}", async (Guid id, GardenDbContext db, HttpContext http) =>
        {
            var userId = http.User.CurrentUserId();
            var plantId = PlantId.From(id);
            var plant = await db.Plants.SingleOrDefaultAsync(plant => plant.Id == plantId && plant.UserId == userId);
            if (plant is null)
            {
                return Results.NotFound();
            }

            _ = db.Plants.Remove(plant);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
