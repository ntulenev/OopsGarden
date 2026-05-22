using Abstractions;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IListInvitesUseCase" />
internal sealed class ListInvitesUseCase : IListInvitesUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListInvitesUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public ListInvitesUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminInvite>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var invites = await _unitOfWork.Invites.ListAsync(cancellationToken).ConfigureAwait(false);
        return [.. invites
            .Select(invite => new AdminInvite(
                invite.Id,
                invite.Code,
                invite.CreatedAt,
                invite.CreatedBy,
                invite.UsedAt,
                invite.UsedByUserId,
                invite.IsRevoked))];
    }

    private readonly IUnitOfWork _unitOfWork;
}
