using FluentAssertions;

namespace Transport.Tests;

public sealed class SettingsRequestTests
{
    [Fact(DisplayName = "Constructor stores settings values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var request = new SettingsRequest("User", "en", "data:image/png;base64,abc", true);

        request.DisplayName.Should().Be("User");
        request.Language.Should().Be("en");
        request.AvatarDataUrl.Should().Be("data:image/png;base64,abc");
        request.IsGardenPublic.Should().BeTrue();
    }
}
