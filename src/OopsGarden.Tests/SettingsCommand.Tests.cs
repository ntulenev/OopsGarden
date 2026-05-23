using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class SettingsCommandTests
{
    [Fact(DisplayName = "Constructor stores settings values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var value = new SettingsCommand("User", "ru", "avatar", true);

        value.DisplayName.Should().Be("User");
        value.Language.Should().Be("ru");
        value.AvatarData.Should().Be("avatar");
        value.IsGardenPublic.Should().BeTrue();
    }
}
