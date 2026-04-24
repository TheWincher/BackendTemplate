using FluentResults;
using MediatR;

namespace Backend.Application.Auth.Commands.Logout;

public sealed record LogoutCommand : IRequest<Result>;