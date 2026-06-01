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
    public async Task<CommandResult> ExecuteAsync(UserId id, bool isBlocked, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return CommandResult.NotFound;
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
        return CommandResult.Succeeded;
    }

    private readonly IUnitOfWork _unitOfWork;
}
