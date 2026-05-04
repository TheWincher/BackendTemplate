using Backend.Application.Abstractions;
using Backend.Domain.Events;
using MediatR;

namespace Backend.Application.Users.Events;

public sealed class SendWelcomeEmailHandler(IUserRepository users/*, IEmailService email*/)
        : INotificationHandler<UserCreatedDomainEvent>  // ← écoute cet event
{
    private readonly IUserRepository _users = users;
    // private readonly IEmailService _email = email;

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(notification.UserId, ct);
        if (user is null) return;

        //TODO: envoyer un email de bienvenue à l'utilisateur
        // await _email.SendAsync(
        //     to: user.Email.Value,
        //     subject: "Bienvenue !",
        //     body: $"Bonjour {user.Username}, votre compte est créé.",
        //     ct);
    }
}