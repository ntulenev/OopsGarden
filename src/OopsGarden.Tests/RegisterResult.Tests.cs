using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class RegisterResultTests
{
    [Fact(DisplayName = "Succeeded creates successful result")]
    [Trait("Category", "Unit")]
    public void SucceededCreatesSuccessfulResult()
    {
        var user = new AuthenticatedUser(UserId.New(), "User", "user@example.com", "en", null, false);

        var value = RegisterResult.Succeeded(user);

        value.IsSuccess.Should().BeTrue();
        value.Status.Should().Be(CommandStatus.Succeeded);
        value.User.Should().Be(user);
        value.Error.Should().BeNull();
    }

    [Fact(DisplayName = "Invalid creates failed result")]
    [Trait("Category", "Unit")]
    public void InvalidCreatesFailedResult()
    {
        var value = RegisterResult.Invalid(RegisterError.InvalidInvite);

        value.IsSuccess.Should().BeFalse();
        value.Status.Should().Be(CommandStatus.Invalid);
        value.User.Should().BeNull();
        value.Error.Should().Be(RegisterError.InvalidInvite);
        value.ErrorMessage.Should().Be("Invalid invite.");
    }
}
