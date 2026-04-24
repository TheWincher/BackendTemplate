using Backend.Application.Abstractions;
using Backend.Application.Users.Dtos;
using Backend.Domain.Errors;
using FluentResults;
using MediatR;

namespace Backend.Application.Auth.Commands.Login;

public sealed class LoginHandler(IUserRepository users, IPasswordHasher hasher)
        : IRequestHandler<LoginCommand, Result<UserDto>>
{
    private readonly IUserRepository _users = users;
    private readonly IPasswordHasher _hasher = hasher;

    public async Task<Result<UserDto>> Handle(
        LoginCommand cmd,
        CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(cmd.Username, ct);

        if (user is null || !_hasher.Verify(cmd.Password, user.Password.Value))
            return Result.Fail<UserDto>(new InvalidCredentialsError());

        return Result.Ok(UserDto.FromDomain(user));
    }
}