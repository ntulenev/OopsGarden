
using Microsoft.AspNetCore.Identity;


namespace Logic.UseCases;

/// <inheritdoc cref="ILoginUseCase" />
public sealed class LoginUseCase : ILoginUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="hasher">The password hasher.</param>
    public LoginUseCase(IUnitOfWork unitOfWork, PasswordHasher<AppUser> hasher)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(hasher);
        _unitOfWork = unitOfWork;
        _hasher = hasher;
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

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash.Value, command.Password);
        return result == PasswordVerificationResult.Failed ? null : AuthUseCaseMapping.ToResponse(user);
    }

    private readonly PasswordHasher<AppUser> _hasher;
    private readonly IUnitOfWork _unitOfWork;
}
