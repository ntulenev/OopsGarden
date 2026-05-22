using System.Security.Claims;

using Abstractions.Repositories;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="IGetMeUseCase" />
public sealed class GetMeUseCase : IGetMeUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMeUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public GetMeUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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
