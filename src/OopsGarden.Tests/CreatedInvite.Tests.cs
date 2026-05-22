
using FluentAssertions;


namespace OopsGarden.Tests;

public sealed class CreatedInviteTests
{
    [Fact(DisplayName = "Constructor stores created invite values")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCalledStoresValues()
    {
        var id = InviteId.New();
        var url = new Uri("https://example.com/?invite=code");

        var value = new CreatedInvite(id, "code", url);

        value.Id.Should().Be(id);
        value.Code.Should().Be("code");
        value.Url.Should().Be(url);
    }
}
