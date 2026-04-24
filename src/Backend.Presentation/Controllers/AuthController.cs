using System.Security.Claims;
using Backend.Application.Auth.Commands.Login;
using Backend.Application.Auth.Commands.Register;
using Backend.Application.Users.Dtos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Backend.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand cmd,
        CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.ToActionResult(dto =>
 
            Ok(result.Value)
    );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand cmd,
        CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return result.ToActionResult(dto =>
        {
            SignInAsync(result.Value).Wait();
            return Ok(result.Value);
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Les claims sont disponibles depuis le cookie
        var id       = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email    = User.FindFirst(ClaimTypes.Email)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new { id, email, username });
    }

    // Méthode privée — crée le cookie de session
    private async Task SignInAsync(UserDto dto)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, dto.Id.ToString()),
            new(ClaimTypes.Email,          dto.Email),
            new(ClaimTypes.Name,           dto.Username)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }
}