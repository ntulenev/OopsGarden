using System.Security.Claims;
using System.Security.Cryptography;


using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="ICreateInviteUseCase" />
internal sealed class CreateInviteUseCase : ICreateInviteUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInviteUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public CreateInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<CreatedInvite> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var bytes = RandomNumberGenerator.GetBytes(24);
        var code = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var invite = InviteLink.Create(
            InviteCode.From(code),
            AdminName.From(principal.Identity?.Name ?? "admin"));
        await _unitOfWork.Invites.AddAsync(invite, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CreatedInvite(invite.Id, invite.Code.Value, new Uri($"/?invite={invite.Code.Value}", UriKind.Relative));
    }

    private readonly IUnitOfWork _unitOfWork;
}
