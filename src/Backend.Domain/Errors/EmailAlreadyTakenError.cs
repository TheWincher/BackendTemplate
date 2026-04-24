using FluentResults;

namespace Backend.Domain.Errors;

public sealed class EmailAlreadyTakenError(string email) : Error($"L'email '{email}' est déjà utilisé.")
{
}