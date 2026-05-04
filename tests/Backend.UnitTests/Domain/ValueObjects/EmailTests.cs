using Backend.Domain.ValueObjects;
using FluentAssertions;

namespace Backend.UnitTests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("ALICE@EXAMPLE.COM")]  // doit être normalisé en lowercase
    [InlineData("alice+tag@example.com")]
    public void Create_WithValidEmail_ShouldSucceed(string value)
    {
        // Act
        var result = Email.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value.ToLowerInvariant().Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldFail(string value)
    {
        var result = Email.Create(value);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Message.Contains("empty") || e.Message.Contains("null"));
    }

    [Fact]
    public void Create_WithoutAtSign_ShouldFail()
    {
        var result = Email.Create("notanemail");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Invalid format"));
    }

    [Fact]
    public void Create_WithEmailTooLong_ShouldFail()
    {
        var longEmail = new string('a', 315) + "@ex.com"; // > 320 chars

        var result = Email.Create(longEmail);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void TwoEmailsWithSameValue_ShouldBeEqual()
    {
        var email1 = Email.Create("alice@example.com").Value;
        var email2 = Email.Create("alice@example.com").Value;

        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void TwoEmailsWithDifferentValues_ShouldNotBeEqual()
    {
        var email1 = Email.Create("alice@example.com").Value;
        var email2 = Email.Create("bob@example.com").Value;

        email1.Should().NotBe(email2);
    }
}