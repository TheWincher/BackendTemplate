using Backend.Domain.Primitives;
using FluentResults;

namespace Backend.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; private set; } = default!;

    private Email() { } 
    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<Email>("Email can't be empty or null");
        if (!value.Contains('@'))
            return Result.Fail<Email>("Invalid format");
        if (value.Length > 320)
            return Result.Fail<Email>("Email too long (max 320 characters).");

        return Result.Ok(new Email(value.ToLowerInvariant().Trim()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}