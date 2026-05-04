using Backend.Domain.Events;
using Backend.Domain.Users;
using FluentAssertions;

namespace Backend.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var result = User.Create("alice", "hashedpassword123", "alice@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("alice");
        result.Value.Email.Value.Should().Be("alice@example.com");
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var result = User.Create("alice", "hashedpassword123", "alice@example.com");

        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedDomainEvent>();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WithCorrectUserId()
    {
        var result = User.Create("alice", "hashedpassword123", "alice@example.com");

        var domainEvent = result.Value.DomainEvents
            .OfType<UserCreatedDomainEvent>()
            .Single();

        domainEvent.UserId.Should().Be(result.Value.Id);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldFail()
    {
        var result = User.Create("alice", "hashedpassword123", "notanemail");

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_ShouldUpdateEmail()
    {
        var user = User.Create("alice", "hash", "alice@example.com").Value;
        user.ClearDomainEvents();

        var result = user.ChangeEmail("newalice@example.com");

        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be("newalice@example.com");
    }

    [Fact]
    public void ChangeEmail_WithInvalidEmail_ShouldFail()
    {
        var user = User.Create("alice", "hash", "alice@example.com").Value;

        var result = user.ChangeEmail("notanemail");

        result.IsFailed.Should().BeTrue();
        user.Email.Value.Should().Be("alice@example.com"); // inchangé
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePassword()
    {
        var user = User.Create("alice", "oldhash", "alice@example.com").Value;

        var result = user.ChangePassword("newhash");

        result.IsSuccess.Should().BeTrue();
        user.Password.Value.Should().Be("newhash");
    }
}