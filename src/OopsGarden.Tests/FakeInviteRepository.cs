using Abstractions;

using Models;

namespace OopsGarden.Tests;

internal sealed class FakeInviteRepository : IInviteRepository
{
    public List<InviteLink> Invites { get; } = [];

    public Task<InviteLink?> FindByCodeAsync(InviteCode code, CancellationToken cancellationToken) =>
        Task.FromResult(Invites.SingleOrDefault(invite => invite.Code == code));

    public Task<InviteLink?> FindByIdAsync(InviteId id, CancellationToken cancellationToken) =>
        Task.FromResult(Invites.SingleOrDefault(invite => invite.Id == id));

    public Task<IReadOnlyList<AdminInviteProjection>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AdminInviteProjection>>([.. Invites.Select(invite => new AdminInviteProjection(
            invite.Id,
            invite.Code.Value,
            invite.CreatedAt,
            invite.CreatedBy.Value,
            invite.UsedAt,
            invite.UsedByUserId,
            invite.IsRevoked))]);

    public Task AddAsync(InviteLink invite, CancellationToken cancellationToken)
    {
        Invites.Add(invite);
        return Task.CompletedTask;
    }

    public void Remove(InviteLink invite) => Invites.Remove(invite);
}
