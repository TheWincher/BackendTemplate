using Backend.Domain.Events;
using Backend.Domain.Primitives;
using Backend.Domain.ValueObjects;
using FluentResults;

namespace Backend.Domain.Users;

public class User : AggregateRoot
{
    public string Username { get; private set; } = null!;
    public HashedPassword Password {get; private set;} = null!;
    public Email Email { get; private set; } = null!;

    private User() { }

    public static Result<User> Create(
        string username,
        string passwordHash,
        string email)
    {
        var emailResult = Email.Create(email);
        var hashedPassword = HashedPassword.FromHash(passwordHash);
        
        var merged = Result.Merge(emailResult, hashedPassword);
        if (merged.IsFailed) return merged;

        User user = new()
        {
            Id = Guid.CreateVersion7(),
            Username = username,
            Password = hashedPassword.Value,
            Email = emailResult.Value,
            CreatedAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
        return Result.Ok(user);
    }

    public Result ChangePassword(string newPasswordHash)
    {
        var hashedPassword = HashedPassword.FromHash(newPasswordHash);
        if (hashedPassword.IsFailed) return Result.Fail(hashedPassword.Errors);

        Password = hashedPassword.Value;
        return Result.Ok();
    }

    public Result ChangeEmail(string newEmail)
    {
        var result = Email.Create(newEmail);
        if (result.IsFailed) return Result.Fail(result.Errors);

        Email = result.Value;
        RaiseDomainEvent(new UserEmailChangedDomainEvent(Id, result.Value.Value));
        return Result.Ok();
    }
}