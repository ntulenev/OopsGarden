using System.Security.Claims;

using Abstractions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Models;
using OopsGarden.Configuration;

namespace OopsGarden.UseCases;

internal sealed class LoginUseCase : ILoginUseCase
{
    public LoginUseCase(IUnitOfWork unitOfWork, PasswordHasher<AppUser> hasher)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(hasher);
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

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

internal sealed class AdminLoginUseCase : IAdminLoginUseCase
{
    public AdminLoginUseCase(IOptions<AdminOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public AdminLogin? Execute(LoginCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var admin = _options.Value.Users.SingleOrDefault(user =>
            string.Equals(user.UserName, command.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        return admin is null || admin.Password != command.Password
            ? null
            : new AdminLogin(admin.UserName, "Admin");
    }

    private readonly IOptions<AdminOptions> _options;
}

internal sealed class RegisterUseCase : IRegisterUseCase
{
    public RegisterUseCase(IUnitOfWork unitOfWork, PasswordHasher<AppUser> hasher)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(hasher);
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

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
            LanguageCode.From(command.Language));
        user.ChangePasswordHash(PasswordHash.From(_hasher.HashPassword(user, command.Password)));
        invite.MarkUsed(user.Id);

        await _unitOfWork.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new RegisterResult(AuthUseCaseMapping.ToResponse(user), null);
    }

    private readonly PasswordHasher<AppUser> _hasher;
    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class UpdateSettingsUseCase : IUpdateSettingsUseCase
{
    public UpdateSettingsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticatedUser?> ExecuteAsync(
        UserId userId,
        SettingsCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var user = await _unitOfWork.Users.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsBlocked)
        {
            return null;
        }

        user.UpdateSettings(
            DisplayName.From(command.DisplayName),
            LanguageCode.From(command.Language),
            ImageDataUrl.Avatar(command.AvatarData),
            command.IsGardenPublic);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return AuthUseCaseMapping.ToResponse(user);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class GetMeUseCase : IGetMeUseCase
{
    public GetMeUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrentUser> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (!principal.Identity?.IsAuthenticated ?? true)
        {
            return new CurrentUser(false);
        }

        var role = principal.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            return new CurrentUser(
                true,
                null,
                principal.Identity?.Name,
                role,
                principal.FindFirstValue("language") ?? "en");
        }

        var userId = CurrentUserId(principal);
        var user = await _unitOfWork.Users.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return user is null
            ? new CurrentUser(false)
            : new CurrentUser(
                true,
                user.Id,
                user.DisplayName.Value,
                role,
                user.Language.Value,
                user.AvatarDataUrl?.Value,
                user.IsGardenPublic);
    }

    private readonly IUnitOfWork _unitOfWork;

    private static UserId CurrentUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? UserId.From(id)
            : throw new InvalidOperationException("Missing user id.");
    }
}

internal static class AuthUseCaseMapping
{
    public static AuthenticatedUser ToResponse(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new AuthenticatedUser(
            user.Id,
            user.DisplayName.Value,
            user.Email.Value,
            user.Language.Value,
            user.AvatarDataUrl?.Value,
            user.IsGardenPublic);
    }
}

