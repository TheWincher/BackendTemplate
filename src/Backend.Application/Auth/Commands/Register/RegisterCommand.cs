using Backend.Application.Users.Dtos;
using FluentResults;
using MediatR;

namespace Backend.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password) : IRequest<Result<UserDto>>;