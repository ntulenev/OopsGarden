using Abstractions.Repositories;

using Models;

namespace Storage.Repositories;

/// <summary>
/// Provides EF Core unit-of-work implementation.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EfUnitOfWork"/> class.
    /// </summary>
    public EfUnitOfWork(
        GardenDbContext dbContext,
        IUserRepository users,
        IInviteRepository invites,
        IPlantRepository plants,
        ILocationRepository locations)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(invites);
        ArgumentNullException.ThrowIfNull(plants);
        ArgumentNullException.ThrowIfNull(locations);
        _dbContext = dbContext;
        Users = users;
        Invites = invites;
        Plants = plants;
        Locations = locations;
        _users = users as UsersRepository;
        _invites = invites as InvitesRepository;
        _plantSync = plants as ISyncChanges;
        _locationSync = locations as ISyncChanges;
    }

    /// <inheritdoc />
    public IUserRepository Users { get; }

    /// <inheritdoc />
    public IInviteRepository Invites { get; }

    /// <inheritdoc />
    public IPlantRepository Plants { get; }

    /// <inheritdoc />
    public ILocationRepository Locations { get; }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _users?.SyncChanges();
        _invites?.SyncChanges();
        _plantSync?.SyncChanges();
        _locationSync?.SyncChanges();
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private readonly GardenDbContext _dbContext;
    private readonly UsersRepository? _users;
    private readonly InvitesRepository? _invites;
    private readonly ISyncChanges? _plantSync;
    private readonly ISyncChanges? _locationSync;
}
