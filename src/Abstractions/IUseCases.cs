using System.Security.Claims;

using Models;

namespace Abstractions;

/// <summary>
/// Defines user login behavior.
/// </summary>
public interface ILoginUseCase
{
    /// <summary>
    /// Authenticates a user.
    /// </summary>
    Task<AuthenticatedUser?> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines admin login behavior.
/// </summary>
public interface IAdminLoginUseCase
{
    /// <summary>
    /// Authenticates an administrator.
    /// </summary>
    AdminLogin? Execute(LoginCommand command);
}

/// <summary>
/// Defines registration behavior.
/// </summary>
public interface IRegisterUseCase
{
    /// <summary>
    /// Registers a user by invite.
    /// </summary>
    Task<RegisterResult> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines settings update behavior.
/// </summary>
public interface IUpdateSettingsUseCase
{
    /// <summary>
    /// Updates user settings.
    /// </summary>
    Task<AuthenticatedUser?> ExecuteAsync(UserId userId, SettingsCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines current-session lookup behavior.
/// </summary>
public interface IGetMeUseCase
{
    /// <summary>
    /// Gets current principal information.
    /// </summary>
    Task<CurrentUser> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

/// <summary>
/// Defines public garden lookup behavior.
/// </summary>
public interface IGetPublicGardenUseCase
{
    /// <summary>
    /// Gets a public garden by owner id.
    /// </summary>
    Task<PublicGarden?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines garden plant list behavior.
/// </summary>
public interface IListGardenPlantsUseCase
{
    /// <summary>
    /// Lists garden plants for a user.
    /// </summary>
    Task<IReadOnlyList<PlantSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken);
}

/// <summary>
/// Defines garden location list behavior.
/// </summary>
public interface IListGardenLocationsUseCase
{
    /// <summary>
    /// Lists garden locations for a user.
    /// </summary>
    Task<IReadOnlyList<LocationSummary>> ExecuteAsync(UserId userId, CancellationToken cancellationToken);
}

/// <summary>
/// Defines location creation behavior.
/// </summary>
public interface ICreateLocationUseCase
{
    /// <summary>
    /// Creates a garden location.
    /// </summary>
    Task<LocationSummary> ExecuteAsync(UserId userId, LocationCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines location rename behavior.
/// </summary>
public interface IRenameLocationUseCase
{
    /// <summary>
    /// Renames a garden location.
    /// </summary>
    Task<LocationSummary?> ExecuteAsync(UserId userId, Guid id, LocationCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines location deletion behavior.
/// </summary>
public interface IDeleteLocationUseCase
{
    /// <summary>
    /// Deletes a garden location.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines plant creation behavior.
/// </summary>
public interface ICreatePlantUseCase
{
    /// <summary>
    /// Creates a plant.
    /// </summary>
    Task<CreatePlantResult> ExecuteAsync(UserId userId, PlantCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines plant update behavior.
/// </summary>
public interface IUpdatePlantUseCase
{
    /// <summary>
    /// Updates a plant.
    /// </summary>
    Task<UpdatePlantResult> ExecuteAsync(UserId userId, Guid id, PlantCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines plant deletion behavior.
/// </summary>
public interface IDeletePlantUseCase
{
    /// <summary>
    /// Deletes a plant.
    /// </summary>
    Task<bool> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines plant watering behavior.
/// </summary>
public interface IWaterPlantUseCase
{
    /// <summary>
    /// Adds a watering event for a plant.
    /// </summary>
    Task<DateTimeOffset?> ExecuteAsync(UserId userId, Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines invite list behavior.
/// </summary>
public interface IListInvitesUseCase
{
    /// <summary>
    /// Lists invites.
    /// </summary>
    Task<IReadOnlyList<AdminInvite>> ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Defines invite creation behavior.
/// </summary>
public interface ICreateInviteUseCase
{
    /// <summary>
    /// Creates an invite.
    /// </summary>
    Task<CreatedInvite> ExecuteAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

/// <summary>
/// Defines invite revoke behavior.
/// </summary>
public interface IRevokeInviteUseCase
{
    /// <summary>
    /// Revokes an invite.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines invite deletion behavior.
/// </summary>
public interface IDeleteInviteUseCase
{
    /// <summary>
    /// Deletes an invite.
    /// </summary>
    Task<DeleteInviteResult> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>
/// Defines admin user list behavior.
/// </summary>
public interface IListUsersUseCase
{
    /// <summary>
    /// Lists users for administration.
    /// </summary>
    Task<IReadOnlyList<AdminUser>> ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Defines user blocking behavior.
/// </summary>
public interface IBlockUserUseCase
{
    /// <summary>
    /// Updates blocked state for a user.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, bool isBlocked, CancellationToken cancellationToken);
}

/// <summary>
/// Defines user deletion behavior.
/// </summary>
public interface IDeleteUserUseCase
{
    /// <summary>
    /// Deletes a user.
    /// </summary>
    Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
