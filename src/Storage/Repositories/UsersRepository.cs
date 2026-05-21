using Abstractions;

using Microsoft.EntityFrameworkCore;

using Models;
using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core user persistence operations.
/// </summary>
public sealed class UsersRepository : IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersRepository"/> class.
    /// </summary>
    public UsersRepository(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<AppUser?> FindByEmailAsync(UserEmail email, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Users
            .SingleOrDefaultAsync(user => user.Email == email.Value, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Track(entity);
    }

    /// <inheritdoc />
    public async Task<AppUser?> FindByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Users.FindAsync([id.Value], cancellationToken).AsTask().ConfigureAwait(false);
        return entity is null ? null : Track(entity);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken) =>
        _dbContext.Users.AnyAsync(user => user.Email == email.Value, cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(AppUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var entity = user.ToEntity();
        _ = _dbContext.Users.Add(entity);
        _tracked[user.Id.Value] = (user, entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminUserProjection>> ListAdminUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .OrderBy(user => user.DisplayName)
            .Select(user => new
            {
                Id = UserId.From(user.Id),
                user.DisplayName,
                user.Email,
                user.IsBlocked,
                user.Language,
                user.CreatedAt,
                Plants = user.Plants.Count
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. users
            .Select(user => new AdminUserProjection(
                user.Id,
                user.DisplayName,
                user.Email,
                user.IsBlocked,
                user.Language,
                user.CreatedAt,
                user.Plants))];
    }

    /// <inheritdoc />
    public void Remove(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!_tracked.TryGetValue(user.Id.Value, out var tracked))
        {
            tracked = (user, user.ToEntity());
            _dbContext.Users.Attach(tracked.Entity);
        }

        _ = _dbContext.Users.Remove(tracked.Entity);
    }

    internal void SyncChanges()
    {
        foreach (var (user, entity) in _tracked.Values)
        {
            user.CopyTo(entity);
        }
    }

    private AppUser Track(AppUserEntity entity)
    {
        var user = entity.ToDomain();
        _tracked[entity.Id] = (user, entity);
        return user;
    }

    private readonly GardenDbContext _dbContext;
    private readonly Dictionary<Guid, (AppUser User, AppUserEntity Entity)> _tracked = [];
}
