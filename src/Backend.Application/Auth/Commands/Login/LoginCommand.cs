using Backend.Application.Users.Dtos;
using FluentResults;
using MediatR;

namespace Backend.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<Result<UserDto>>;