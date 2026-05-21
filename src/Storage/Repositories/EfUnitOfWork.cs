using Abstractions;

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
        IGardenRepository garden)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(invites);
        ArgumentNullException.ThrowIfNull(garden);
        _dbContext = dbContext;
        Users = users;
        Invites = invites;
        Garden = garden;
        _users = users as UsersRepository;
        _invites = invites as InvitesRepository;
        _garden = garden as GardenRepository;
    }

    /// <inheritdoc />
    public IUserRepository Users { get; }

    /// <inheritdoc />
    public IInviteRepository Invites { get; }

    /// <inheritdoc />
    public IGardenRepository Garden { get; }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _users?.SyncChanges();
        _invites?.SyncChanges();
        _garden?.SyncChanges();
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private readonly GardenDbContext _dbContext;
    private readonly UsersRepository? _users;
    private readonly InvitesRepository? _invites;
    private readonly GardenRepository? _garden;
}
