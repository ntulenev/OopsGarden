using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IDeleteInviteUseCase" />
public sealed class DeleteInviteUseCase : IDeleteInviteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteInviteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public DeleteInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<DeleteInviteResult> ExecuteAsync(InviteId id, CancellationToken cancellationToken)
    {
        var invite = await _unitOfWork.Invites.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (invite is null)
        {
            return new DeleteInviteResult(DeleteInviteStatus.NotFound, null);
        }

        if (invite.UsedAt is not null)
        {
            return new DeleteInviteResult(DeleteInviteStatus.Invalid, DeleteInviteError.UsedInviteCannotBeDeleted);
        }

        _unitOfWork.Invites.Remove(invite);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new DeleteInviteResult(DeleteInviteStatus.Deleted, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}
