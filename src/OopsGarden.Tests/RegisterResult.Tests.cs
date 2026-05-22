
using FluentAssertions;

using Models;

namespace OopsGarden.Tests;

public sealed class RegisterResultTests
{
    [Fact(DisplayName = "IsSuccess returns true when error is missing")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorIsMissingReturnsTrue()
    {
        var user = new AuthenticatedUser(UserId.New(), "User", "user@example.com", "en", null, false);

        var value = new RegisterResult(user, null);

        value.IsSuccess.Should().BeTrue();
        value.User.Should().Be(user);
    }

    [Fact(DisplayName = "IsSuccess returns false when error exists")]
    [Trait("Category", "Unit")]
    public void IsSuccessWhenErrorExistsReturnsFalse()
    {
        var value = new RegisterResult(null, "Invalid invite.");

        value.IsSuccess.Should().BeFalse();
        value.Error.Should().Be("Invalid invite.");
    }
}
