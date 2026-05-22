
using Microsoft.Extensions.Options;

using Logic.Configuration;

namespace Logic.UseCases;

/// <inheritdoc cref="IAdminLoginUseCase" />
public sealed class AdminLoginUseCase : IAdminLoginUseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLoginUseCase"/> class.
    /// </summary>
    /// <param name="options">The configured administrator credentials.</param>
    public AdminLoginUseCase(IOptions<AdminOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
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
