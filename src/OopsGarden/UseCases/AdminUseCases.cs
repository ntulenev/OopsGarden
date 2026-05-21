using System.Security.Claims;
using System.Security.Cryptography;

using Abstractions;

using Models;

namespace OopsGarden.UseCases;

internal sealed class ListInvitesUseCase : IListInvitesUseCase
{
    public ListInvitesUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AdminInvite>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var invites = await _unitOfWork.Invites.ListAsync(cancellationToken).ConfigureAwait(false);
        return [.. invites
            .Select(invite => new AdminInvite(
                invite.Id,
                invite.Code,
                invite.CreatedAt,
                invite.CreatedBy,
                invite.UsedAt,
                invite.UsedByUserId,
                invite.IsRevoked))];
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class CreateInviteUseCase : ICreateInviteUseCase
{
    public CreateInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

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

internal sealed class RevokeInviteUseCase : IRevokeInviteUseCase
{
    public RevokeInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var invite = await _unitOfWork.Invites.FindByIdAsync(InviteId.From(id), cancellationToken).ConfigureAwait(false);
        if (invite is null)
        {
            return false;
        }

        invite.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class DeleteInviteUseCase : IDeleteInviteUseCase
{
    public DeleteInviteUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteInviteResult> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var invite = await _unitOfWork.Invites.FindByIdAsync(InviteId.From(id), cancellationToken).ConfigureAwait(false);
        if (invite is null)
        {
            return new DeleteInviteResult(DeleteInviteStatus.NotFound, null);
        }

        if (invite.UsedAt is not null)
        {
            return new DeleteInviteResult(DeleteInviteStatus.Invalid, "Used invite cannot be deleted.");
        }

        _unitOfWork.Invites.Remove(invite);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new DeleteInviteResult(DeleteInviteStatus.Deleted, null);
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class ListUsersUseCase : IListUsersUseCase
{
    public ListUsersUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AdminUser>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.ListAdminUsersAsync(cancellationToken).ConfigureAwait(false);
        return [.. users
            .Select(user => new AdminUser(
                user.Id,
                user.DisplayName,
                user.Email,
                user.IsBlocked,
                user.Language,
                user.CreatedAt,
                user.Plants))];
    }

    private readonly IUnitOfWork _unitOfWork;
}

internal sealed class BlockUserUseCase : IBlockUserUseCase
{
    public BlockUserUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

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

internal sealed class DeleteUserUseCase : IDeleteUserUseCase
{
    public DeleteUserUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByIdAsync(UserId.From(id), cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private readonly IUnitOfWork _unitOfWork;
}

