using Abstractions.Repositories;

using Microsoft.EntityFrameworkCore;

using Models;

using Storage.Entities;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core invite persistence operations.
/// </summary>
public sealed class InvitesRepository : IInviteRepository, ISyncChanges
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvitesRepository"/> class.
    /// </summary>
    public InvitesRepository(GardenDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<InviteLink?> FindByCodeAsync(InviteCode code, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Invites
            .SingleOrDefaultAsync(invite => invite.Code == code.Value, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Track(entity);
    }

    /// <inheritdoc />
    public async Task<InviteLink?> FindByIdAsync(InviteId id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Invites.FindAsync([id.Value], cancellationToken).AsTask().ConfigureAwait(false);
        return entity is null ? null : Track(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminInviteProjection>> ListAsync(CancellationToken cancellationToken)
    {
        var invites = await _dbContext.Invites
            .OrderByDescending(invite => invite.CreatedAt)
            .Select(invite => new
            {
                Id = InviteId.From(invite.Id),
                invite.Code,
                invite.CreatedAt,
                invite.CreatedBy,
                invite.UsedAt,
                UsedByUserId = invite.UsedByUserId.HasValue ? UserId.From(invite.UsedByUserId.Value) : (UserId?)null,
                invite.IsRevoked
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. invites
            .Select(invite => new AdminInviteProjection(
                invite.Id,
                invite.Code,
                invite.CreatedAt,
                invite.CreatedBy,
                invite.UsedAt,
                invite.UsedByUserId,
                invite.IsRevoked))];
    }

    /// <inheritdoc />
    public Task AddAsync(InviteLink invite, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invite);
        var entity = invite.ToEntity();
        _ = _dbContext.Invites.Add(entity);
        _tracked[invite.Id.Value] = (invite, entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Remove(InviteLink invite)
    {
        ArgumentNullException.ThrowIfNull(invite);
        if (!_tracked.TryGetValue(invite.Id.Value, out var tracked))
        {
            tracked = (invite, invite.ToEntity());
            _dbContext.Invites.Attach(tracked.Entity);
        }

        _ = _dbContext.Invites.Remove(tracked.Entity);
    }

    /// <inheritdoc />
    public void SyncChanges()
    {
        foreach (var (invite, entity) in _tracked.Values)
        {
            invite.CopyTo(entity);
        }
    }

    private InviteLink Track(InviteLinkEntity entity)
    {
        var invite = entity.ToDomain();
        _tracked[entity.Id] = (invite, entity);
        return invite;
    }

    private readonly GardenDbContext _dbContext;
    private readonly Dictionary<Guid, (InviteLink Invite, InviteLinkEntity Entity)> _tracked = [];
}
