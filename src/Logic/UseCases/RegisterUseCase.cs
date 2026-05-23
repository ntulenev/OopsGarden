using Abstractions.Repositories;
using Abstractions.Security;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IRegisterUseCase" />
public sealed class RegisterUseCase : IRegisterUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="passwords">The password service.</param>
    /// <param name="clock">The application clock.</param>
    public RegisterUseCase(IUnitOfWork unitOfWork, IPasswordService passwords, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(passwords);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _passwords = passwords;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<RegisterResult> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var email = UserEmail.From(command.Email);
        var inviteCode = InviteCode.From(command.InviteCode);
        var invite = await _unitOfWork.Invites.FindByCodeAsync(inviteCode, cancellationToken).ConfigureAwait(false);
        if (invite is null || !invite.CanBeUsed)
        {
            return new RegisterResult(null, "Invalid invite.");
        }

        if (await _unitOfWork.Users.ExistsByEmailAsync(email, cancellationToken).ConfigureAwait(false))
        {
            return new RegisterResult(null, "Email already registered.");
        }

        var user = AppUser.Create(
            email,
            DisplayName.From(command.DisplayName),
            PasswordHash.From("pending"),
            LanguageCode.From(command.Language),
            _clock.UtcNow);
        user.ChangePasswordHash(_passwords.HashPassword(user, command.Password));
        invite.MarkUsed(user.Id, _clock.UtcNow);

        await _unitOfWork.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new RegisterResult(AuthUseCaseMapping.ToResponse(user), null);
    }

    private readonly IPasswordService _passwords;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
