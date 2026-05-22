using Abstractions;

namespace OopsGarden.Tests;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeUnitOfWork()
    {
        Users = new FakeUserRepository();
        Invites = new FakeInviteRepository();
        Garden = new FakeGardenRepository();
    }

    public FakeUserRepository Users { get; }

    public FakeInviteRepository Invites { get; }

    public FakeGardenRepository Garden { get; }

    IUserRepository IUnitOfWork.Users => Users;

    IInviteRepository IUnitOfWork.Invites => Invites;

    IGardenRepository IUnitOfWork.Garden => Garden;

    public int SaveChangesCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }
}
