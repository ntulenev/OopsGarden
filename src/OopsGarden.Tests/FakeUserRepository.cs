using Abstractions;

using Models;

namespace OopsGarden.Tests;

internal sealed class FakeUserRepository : IUserRepository
{
    public List<AppUser> Users { get; } = [];

    public Task<AppUser?> FindByEmailAsync(UserEmail email, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Email == email));

    public Task<AppUser?> FindByIdAsync(UserId id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Exists(user => user.Email == email));

    public Task AddAsync(AppUser user, CancellationToken cancellationToken)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AdminUserProjection>> ListAdminUsersAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AdminUserProjection>>([.. Users.Select(user => new AdminUserProjection(
            user.Id,
            user.DisplayName.Value,
            user.Email.Value,
            user.IsBlocked,
            user.Language.Value,
            user.CreatedAt,
            user.Plants.Count))]);

    public void Remove(AppUser user) => Users.Remove(user);
}
