using Abstractions.Repositories;
using Abstractions.Security;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="ILoginUseCase" />
public sealed class LoginUseCase : ILoginUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="passwords">The password service.</param>
    public LoginUseCase(IUnitOfWork unitOfWork, IPasswordService passwords)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(passwords);
        _unitOfWork = unitOfWork;
        _passwords = passwords;
    }

    /// <inheritdoc />
    public async Task<AuthenticatedUser?> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var email = UserEmail.From(command.Email);
        var user = await _unitOfWork.Users.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsBlocked)
        {
            return null;
        }

        return _passwords.VerifyPassword(user, command.Password) ? AuthUseCaseMapping.ToResponse(user) : null;
    }

    private readonly IPasswordService _passwords;
    private readonly IUnitOfWork _unitOfWork;
}
