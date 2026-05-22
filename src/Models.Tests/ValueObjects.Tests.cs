using FluentAssertions;

namespace Models.Tests;

public sealed class ValueObjectsTests
{
    [Theory(DisplayName = "Required text value objects trim valid values")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(RequiredTextCases))]
    public void RequiredTextValueObjectsWhenValueIsValidTrimValue(Func<string, string> create)
    {
        // Act
        var value = create("  valid@example.com  ");

        // Assert
        value.Should().NotStartWith(" ");
        value.Should().NotEndWith(" ");
    }

    [Theory(DisplayName = "Required text value objects throw when value is null")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(RequiredTextCases))]
    public void RequiredTextValueObjectsWhenValueIsNullThrowArgumentNullException(Func<string, string> create)
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = create(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory(DisplayName = "Required text value objects throw when value is empty or whitespace")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(RequiredTextInvalidCases))]
    public void RequiredTextValueObjectsWhenValueIsWhitespaceThrowArgumentException(
        Func<string, string> create,
        string value)
    {
        // Act
        Action act = () => _ = create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "Required text value objects throw when value is too long")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(RequiredTextTooLongCases))]
    public void RequiredTextValueObjectsWhenValueIsTooLongThrowArgumentException(
        Func<string, string> create,
        string value)
    {
        // Act
        Action act = () => _ = create(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "Strong ids throw when value is empty")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(EmptyIdCases))]
    public void StrongIdsWhenValueIsEmptyThrowArgumentException(Action create)
    {
        // Act
        var act = create;

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "Strong ids create new non-empty values")]
    [Trait("Category", "Unit")]
    [MemberData(nameof(NewIdCases))]
    public void StrongIdsWhenNewIsCalledCreateNonEmptyValues(Func<Guid> create)
    {
        // Act
        var value = create();

        // Assert
        value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "UserEmail normalizes value and requires at sign")]
    [Trait("Category", "Unit")]
    public void UserEmailWhenValueIsValidNormalizesValue()
    {
        // Act
        var email = UserEmail.From("  user@example.com  ");

        // Assert
        email.Value.Should().Be("USER@EXAMPLE.COM");
    }

    [Fact(DisplayName = "UserEmail throws when value has no at sign")]
    [Trait("Category", "Unit")]
    public void UserEmailWhenValueHasNoAtSignThrowsArgumentException()
    {
        // Act
        Action act = () => _ = UserEmail.From("user.example.com");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "LanguageCode accepts supported values and defaults to English")]
    [Trait("Category", "Unit")]
    [InlineData(null, "en")]
    [InlineData("", "en")]
    [InlineData(" ", "en")]
    [InlineData("EN", "en")]
    [InlineData("ru", "ru")]
    public void LanguageCodeWhenValueIsSupportedNormalizesValue(string? value, string expected)
    {
        // Act
        var language = LanguageCode.From(value);

        // Assert
        language.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "LanguageCode defaults unsupported values to English")]
    [Trait("Category", "Unit")]
    public void LanguageCodeWhenValueIsUnsupportedDefaultsToEnglish()
    {
        // Act
        var language = LanguageCode.From("de");

        // Assert
        language.Value.Should().Be("en");
    }

    [Fact(DisplayName = "LanguageCode exposes supported language constants")]
    [Trait("Category", "Unit")]
    public void LanguageCodeWhenConstantsAreReadReturnsSupportedCodes()
    {
        // Assert
        LanguageCode.English.Should().Be("en");
        LanguageCode.Russian.Should().Be("ru");
    }

    [Theory(DisplayName = "PlantDescription normalizes optional values")]
    [Trait("Category", "Unit")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  green  ", "green")]
    public void PlantDescriptionWhenValueIsOptionalNormalizesValue(string? value, string expected)
    {
        // Act
        var description = PlantDescription.From(value);

        // Assert
        description.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "PlantDescription throws when value is too long")]
    [Trait("Category", "Unit")]
    public void PlantDescriptionWhenValueIsTooLongThrowsArgumentException()
    {
        // Act
        Action act = () => _ = PlantDescription.From(new string('x', PlantDescription.MaxLength + 1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory(DisplayName = "ImageDataUrl returns null for empty optional values")]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ImageDataUrlWhenOptionalValueIsEmptyReturnsNull(string? value)
    {
        // Act
        var avatar = ImageDataUrl.Avatar(value);
        var plantPhoto = ImageDataUrl.PlantPhoto(value);

        // Assert
        avatar.Should().BeNull();
        plantPhoto.Should().BeNull();
    }

    [Fact(DisplayName = "ImageDataUrl accepts image data urls")]
    [Trait("Category", "Unit")]
    public void ImageDataUrlWhenValueIsImageDataUrlCreatesValue()
    {
        // Act
        var avatar = ImageDataUrl.Avatar("  data:image/png;base64,abc  ");

        // Assert
        avatar.Should().NotBeNull();
        avatar!.Value.Value.Should().Be("data:image/png;base64,abc");
    }

    [Fact(DisplayName = "ImageDataUrl throws when value is not image data url")]
    [Trait("Category", "Unit")]
    public void ImageDataUrlWhenValueIsNotImageDataUrlThrowsArgumentException()
    {
        // Act
        Action act = () => _ = ImageDataUrl.Avatar("data:text/plain;base64,abc");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "ImageDataUrl throws when avatar is too long")]
    [Trait("Category", "Unit")]
    public void ImageDataUrlWhenAvatarIsTooLongThrowsArgumentException()
    {
        // Arrange
        var value = "data:image/png;base64," + new string('x', ImageDataUrl.MaxAvatarLength);

        // Act
        Action act = () => _ = ImageDataUrl.Avatar(value);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    public static TheoryData<Func<string, string>> RequiredTextCases =>
        new()
        {
            value => AdminName.From(value).Value,
            value => DisplayName.From(value).Value,
            value => InviteCode.From(value).Value,
            value => LocationName.From(value).Value,
            value => PasswordHash.From(value).Value,
            value => PlantName.From(value).Value,
            value => UserEmail.From(value).Value
        };

    public static TheoryData<Func<string, string>, string> RequiredTextInvalidCases =>
        new()
        {
            { value => AdminName.From(value).Value, string.Empty },
            { value => DisplayName.From(value).Value, " " },
            { value => InviteCode.From(value).Value, "   " },
            { value => LocationName.From(value).Value, "\t" },
            { value => PasswordHash.From(value).Value, string.Empty },
            { value => PlantName.From(value).Value, " " },
            { value => UserEmail.From(value).Value, "   " }
        };

    public static TheoryData<Func<string, string>, string> RequiredTextTooLongCases =>
        new()
        {
            { value => DisplayName.From(value).Value, new string('x', DisplayName.MaxLength + 1) },
            { value => InviteCode.From(value).Value, new string('x', InviteCode.MaxLength + 1) },
            { value => LocationName.From(value).Value, new string('x', LocationName.MaxLength + 1) },
            { value => PlantName.From(value).Value, new string('x', PlantName.MaxLength + 1) },
            { value => UserEmail.From(value).Value, new string('x', UserEmail.MaxLength) + "@example.com" }
        };

    public static TheoryData<Action> EmptyIdCases =>
        new()
        {
            () => _ = InviteId.From(Guid.Empty),
            () => _ = LocationId.From(Guid.Empty),
            () => _ = PlantId.From(Guid.Empty),
            () => _ = UserId.From(Guid.Empty),
            () => _ = WateringEventId.From(Guid.Empty)
        };

    public static TheoryData<Func<Guid>> NewIdCases =>
        new()
        {
            () => InviteId.New().Value,
            () => LocationId.New().Value,
            () => PlantId.New().Value,
            () => UserId.New().Value,
            () => WateringEventId.New().Value
        };
}
