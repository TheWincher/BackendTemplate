using Backend.Application.Abstractions;
using Backend.Application.Users.Dtos;
using Backend.Domain.Errors;
using Backend.Domain.Users;
using FluentResults;
using MediatR;

namespace Backend.Application.Auth.Commands.Register;

public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IUnitOfWork uow)
        : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IUserRepository _users = users;
    private readonly IPasswordHasher _hasher = hasher;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<UserDto>> Handle(
        RegisterCommand cmd,
        CancellationToken ct)
    {
        if (await _users.GetByEmailAsync(cmd.Email, ct) is not null)
            return Result.Fail<UserDto>(new EmailAlreadyTakenError(cmd.Email));

        if (await _users.GetByUsernameAsync(cmd.Username, ct) is not null)
            return Result.Fail<UserDto>(new UsernameAlreadyTakenError(cmd.Username));

        var userResult = User.Create(
            cmd.Username,
            _hasher.Hash(cmd.Password),
            cmd.Email);

        if (userResult.IsFailed)
            return Result.Fail(userResult.Errors);

        await _users.AddAsync(userResult.Value, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Ok(UserDto.FromDomain(userResult.Value));
    }
}