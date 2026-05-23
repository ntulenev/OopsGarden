using Abstractions.UseCases;
using Abstractions.Security;

using Logic.UseCases;

using Microsoft.AspNetCore.Identity;

using Models;

namespace OopsGarden.Startup;

/// <summary>
/// Provides application service registration extensions.
/// </summary>
internal static class ServiceCollectionApplicationExtensions
{
    /// <summary>
    /// Registers OopsGarden application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddOopsGardenApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddScoped<PasswordHasher<AppUser>>();
        _ = services.AddScoped<IPasswordService, IdentityPasswordService>();
        _ = services.AddScoped<ILoginUseCase, LoginUseCase>();
        _ = services.AddScoped<IAdminLoginUseCase, AdminLoginUseCase>();
        _ = services.AddScoped<IRegisterUseCase, RegisterUseCase>();
        _ = services.AddScoped<IUpdateSettingsUseCase, UpdateSettingsUseCase>();
        _ = services.AddScoped<IGetMeUseCase, GetMeUseCase>();
        _ = services.AddScoped<IGetPublicGardenUseCase, GetPublicGardenUseCase>();
        _ = services.AddScoped<IListGardenPlantsUseCase, ListGardenPlantsUseCase>();
        _ = services.AddScoped<IListGardenLocationsUseCase, ListGardenLocationsUseCase>();
        _ = services.AddScoped<ICreateLocationUseCase, CreateLocationUseCase>();
        _ = services.AddScoped<IRenameLocationUseCase, RenameLocationUseCase>();
        _ = services.AddScoped<IDeleteLocationUseCase, DeleteLocationUseCase>();
        _ = services.AddScoped<ICreatePlantUseCase, CreatePlantUseCase>();
        _ = services.AddScoped<IUpdatePlantUseCase, UpdatePlantUseCase>();
        _ = services.AddScoped<IDeletePlantUseCase, DeletePlantUseCase>();
        _ = services.AddScoped<IWaterPlantUseCase, WaterPlantUseCase>();
        _ = services.AddScoped<IListPlantNotesUseCase, ListPlantNotesUseCase>();
        _ = services.AddScoped<ICreatePlantNoteUseCase, CreatePlantNoteUseCase>();
        _ = services.AddScoped<IDeletePlantNoteUseCase, DeletePlantNoteUseCase>();
        _ = services.AddScoped<IListInvitesUseCase, ListInvitesUseCase>();
        _ = services.AddScoped<ICreateInviteUseCase, CreateInviteUseCase>();
        _ = services.AddScoped<IRevokeInviteUseCase, RevokeInviteUseCase>();
        _ = services.AddScoped<IDeleteInviteUseCase, DeleteInviteUseCase>();
        _ = services.AddScoped<IListUsersUseCase, ListUsersUseCase>();
        _ = services.AddScoped<IBlockUserUseCase, BlockUserUseCase>();
        _ = services.AddScoped<IDeleteUserUseCase, DeleteUserUseCase>();

        return services;
    }
}
