using System.Security.Claims;
using System.Security.Cryptography;

using Abstractions.Repositories;
using Abstractions.System;
using Abstractions.UseCases;

using Models;

namespace Logic.UseCases;

/// <inheritdoc cref="ICreateInviteUseCase" />
public sealed class CreateInviteUseCase : ICreateInviteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInviteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    /// <param name="clock">The application clock.</param>
    public CreateInviteUseCase(IUnitOfWork unitOfWork, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(clock);
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<CreatedInvite> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var bytes = RandomNumberGenerator.GetBytes(24);
        var code = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var invite = InviteLink.Create(
            InviteCode.From(code),
            AdminName.From(principal.Identity?.Name ?? "admin"),
            _clock.UtcNow);
        await _unitOfWork.Invites.AddAsync(invite, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CreatedInvite(invite.Id, invite.Code.Value, new Uri($"/?invite={invite.Code.Value}", UriKind.Relative));
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
}
