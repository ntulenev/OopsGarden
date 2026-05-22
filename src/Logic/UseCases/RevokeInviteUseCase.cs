
using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IRevokeInviteUseCase" />
public sealed class RevokeInviteUseCase : IRevokeInviteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevokeInviteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public RevokeInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var invite = await _unitOfWork.Invites.FindByIdAsync(InviteId.From(id), cancellationToken).ConfigureAwait(false);
        if (invite is null)
        {
            return false;
        }

        invite.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
