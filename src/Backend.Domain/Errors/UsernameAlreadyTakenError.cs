using FluentResults;

namespace Backend.Domain.Errors;

public sealed class UsernameAlreadyTakenError(string username) : Error($"Le nom d'utilisateur '{username}' est déjà utilisé.")
{
}