using Backend.Application.Abstractions;
using Backend.Application.Users.Dtos;
using Backend.Domain.Errors;
using FluentResults;
using MediatR;

namespace Backend.Application.Users.Queries.GetUserByIdQuery;

public sealed class GetUserByIdHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _users;

    public GetUserByIdHandler(IUserRepository users)
        => _users = users;

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery query,
        CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.Id, ct);

        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User", query.Id));

        return Result.Ok(UserDto.FromDomain(user));
    }
}