using FluentResults;

namespace Backend.Domain.Errors;

public sealed class InvalidCredentialsError : Error
{
    public InvalidCredentialsError()
        : base("Email ou mot de passe incorrect.") { }
}