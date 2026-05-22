using Abstractions;

using Models;

namespace OopsGarden.UseCases;

/// <inheritdoc cref="IUpdateSettingsUseCase" />
internal sealed class UpdateSettingsUseCase : IUpdateSettingsUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettingsUseCase"/> class.
    /// </summary>
    /// <param name="unitOfWork">The persistence unit of work.</param>
    public UpdateSettingsUseCase(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
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
