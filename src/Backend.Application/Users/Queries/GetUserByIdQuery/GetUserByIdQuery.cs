using Backend.Application.Users.Dtos;
using FluentResults;
using MediatR;

namespace Backend.Application.Users.Queries.GetUserByIdQuery;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;