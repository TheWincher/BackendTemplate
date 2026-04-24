using Backend.Domain.Exceptions;
using Backend.Domain.Primitives;
using FluentResults;

namespace Backend.Domain.ValueObjects;
public sealed class HashedPassword: ValueObject
{
    public string Value {get; private set;} = default!;

    private HashedPassword() { }
    private HashedPassword(string value) => Value = value;

    public static Result<HashedPassword> FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result.Fail(new DomainException("Hashed password can't be null or empty"));
        }

        return Result.Ok(new HashedPassword(hash));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}