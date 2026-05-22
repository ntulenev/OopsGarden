using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IBlockUserUseCase" />
public sealed class BlockUserUseCase : IBlockUserUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockUserUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public BlockUserUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(Guid id, bool isBlocked, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByIdAsync(UserId.From(id), cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        if (isBlocked)
        {
            user.Block();
        }
        else
        {
            user.Unblock();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}
