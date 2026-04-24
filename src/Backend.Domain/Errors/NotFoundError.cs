using FluentResults;

namespace Backend.Domain.Errors;

public sealed class NotFoundError(string resource, object id) : Error($"{resource} avec l'id '{id}' est introuvable.")
{
}