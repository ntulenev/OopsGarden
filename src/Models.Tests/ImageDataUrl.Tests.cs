using FluentAssertions;

namespace Models.Tests;

public sealed class ImageDataUrlTests
{
    [Theory(DisplayName = "Avatar returns null for empty optional value")]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AvatarWhenOptionalValueIsEmptyReturnsNull(string? value)
    {
        var avatar = ImageDataUrl.Avatar(value);

        avatar.Should().BeNull();
    }

    [Theory(DisplayName = "PlantPhoto returns null for empty optional value")]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PlantPhotoWhenOptionalValueIsEmptyReturnsNull(string? value)
    {
        var plantPhoto = ImageDataUrl.PlantPhoto(value);

        plantPhoto.Should().BeNull();
    }

    [Fact(DisplayName = "Avatar accepts image data url")]
    [Trait("Category", "Unit")]
    public void AvatarWhenValueIsImageDataUrlCreatesValue()
    {
        var avatar = ImageDataUrl.Avatar("  data:image/png;base64,abc  ");

        avatar.Should().NotBeNull();
        avatar!.Value.Value.Should().Be("data:image/png;base64,abc");
    }

    [Fact(DisplayName = "Avatar throws when value is not image data url")]
    [Trait("Category", "Unit")]
    public void AvatarWhenValueIsNotImageDataUrlThrowsArgumentException()
    {
        Action act = () => _ = ImageDataUrl.Avatar("data:text/plain;base64,abc");

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Avatar throws when value is too long")]
    [Trait("Category", "Unit")]
    public void AvatarWhenValueIsTooLongThrowsArgumentException()
    {
        var value = "data:image/png;base64," + new string('x', ImageDataUrl.MaxAvatarLength);

        Action act = () => _ = ImageDataUrl.Avatar(value);

        act.Should().Throw<ArgumentException>();
    }
}
