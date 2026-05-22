using FluentAssertions;

namespace Transport.Tests;

public sealed class MeResponseTests
{
    [Fact(DisplayName = "Constructor defaults optional values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOnlyAuthenticatedIsProvidedDefaultsOptionalValues()
    {
        var response = new MeResponse(false);

        response.Authenticated.Should().BeFalse();
        response.Id.Should().BeNull();
        response.Name.Should().BeNull();
        response.Role.Should().BeNull();
        response.Language.Should().BeNull();
        response.Avatar.Should().BeNull();
        response.IsGardenPublic.Should().BeFalse();
    }
}
