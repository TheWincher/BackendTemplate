using FluentResults;

namespace Backend.Domain.Exceptions;

public class DomainException(string? message) : Error(message)
{
    
}