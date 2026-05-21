using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

using Models;
using Storage;
using Transport;

namespace OopsGarden.Endpoints;

internal static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/invites", async (GardenDbContext db) =>
            await db.Invites
                .OrderByDescending(invite => invite.CreatedAt)
                .Select(invite => new
                {
                    Id = invite.Id.Value,
                    Code = invite.Code.Value,
                    invite.CreatedAt,
                    CreatedBy = invite.CreatedBy.Value,
                    invite.UsedAt,
                    UsedByUserId = invite.UsedByUserId.HasValue
                        ? invite.UsedByUserId.Value.Value
                        : (Guid?)null,
                    invite.IsRevoked
                })
                .ToListAsync());

        group.MapPost("/invites", async (GardenDbContext db, ClaimsPrincipal principal) =>
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            var code = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            var invite = InviteLink.Create(
                InviteCode.From(code),
                AdminName.From(principal.Identity?.Name ?? "admin"));
            _ = db.Invites.Add(invite);
            await db.SaveChangesAsync();
            return Results.Ok(new
            {
                Id = invite.Id.Value,
                Code = invite.Code.Value,
                Url = $"/?invite={invite.Code.Value}"
            });
        });

        group.MapPost("/invites/{id:guid}/revoke", async (Guid id, GardenDbContext db) =>
        {
            var invite = await db.Invites.FindAsync(InviteId.From(id));
            if (invite is null)
            {
                return Results.NotFound();
            }

            invite.Revoke();
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/invites/{id:guid}", async (Guid id, GardenDbContext db) =>
        {
            var invite = await db.Invites.FindAsync(InviteId.From(id));
            if (invite is null)
            {
                return Results.NotFound();
            }

            if (invite.UsedAt is not null)
            {
                return Results.BadRequest(new { error = "Used invite cannot be deleted." });
            }

            _ = db.Invites.Remove(invite);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapGet("/users", async (GardenDbContext db) =>
            await db.Users
                .OrderBy(user => user.DisplayName)
                .Select(user => new
                {
                    Id = user.Id.Value,
                    DisplayName = user.DisplayName.Value,
                    Email = user.Email.Value,
                    user.IsBlocked,
                    Language = user.Language.Value,
                    user.CreatedAt,
                    Plants = user.Plants.Count
                })
                .ToListAsync());

        group.MapPost("/users/{id:guid}/block", async (Guid id, BlockUserRequest request, GardenDbContext db) =>
        {
            var user = await db.Users.FindAsync(UserId.From(id));
            if (user is null)
            {
                return Results.NotFound();
            }

            if (request.IsBlocked)
            {
                user.Block();
            }
            else
            {
                user.Unblock();
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/users/{id:guid}", async (Guid id, GardenDbContext db) =>
        {
            var user = await db.Users.FindAsync(UserId.From(id));
            if (user is null)
            {
                return Results.NotFound();
            }

            _ = db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
