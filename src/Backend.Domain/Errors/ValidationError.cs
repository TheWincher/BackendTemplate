using FluentResults;

namespace Backend.Domain.Errors;

public sealed class ValidationError(string message) : Error(message)
{
}