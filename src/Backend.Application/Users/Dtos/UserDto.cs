using Backend.Domain.Users;

namespace Backend.Application.Users.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Username,
    string Password,
    DateTime CreatedAt)
{
    public static UserDto FromDomain(User user) => new(
        user.Id,
        user.Email.Value,
        user.Username,
        user.Password.Value,
        user.CreatedAt);
}