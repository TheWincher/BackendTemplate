# BackendTemplate

Template de backend .NET en **Clean Architecture** avec DDD (Domain-Driven Design), CQRS, et authentification par cookie de session.

---

## Table des matières

- [Architecture](#architecture)
- [Structure du projet](#structure-du-projet)
- [Concepts clés](#concepts-clés)
  - [Clean Architecture](#clean-architecture)
  - [Domain-Driven Design](#domain-driven-design)
  - [CQRS avec MediatR](#cqrs-avec-mediatr)
  - [Value Objects](#value-objects)
  - [AggregateRoot](#aggregateroot)
  - [Domain Events](#domain-events)
  - [Unit of Work](#unit-of-work)
  - [FluentResults](#fluentresults)
  - [Authentification par cookie](#authentification-par-cookie)
- [Technologies](#technologies)
- [Démarrage rapide](#démarrage-rapide)
- [Variables d'environnement](#variables-denvironnement)

---

## Architecture

Ce projet suit les principes de la **Clean Architecture** de Robert C. Martin. Les dépendances vont toujours vers l'intérieur — le Domain ne dépend de rien, l'Application dépend uniquement du Domain, et l'Infrastructure et la Presentation dépendent de l'Application.

```
┌─────────────────────────────────────────┐
│              Presentation               │  Controllers, Middlewares
├─────────────────────────────────────────┤
│             Infrastructure              │  EF Core, Repositories, Services
├─────────────────────────────────────────┤
│              Application                │  Handlers, Commands, Queries, DTOs
├─────────────────────────────────────────┤
│                Domain                   │  Entités, Value Objects, Domain Events
└─────────────────────────────────────────┘

Règle fondamentale : les dépendances pointent toujours vers l'intérieur.
```

---

## Structure du projet

```
src/
├── Backend.Domain/              # Cœur métier — aucune dépendance externe
│   ├── Primitives/
│   │   ├── Entity.cs            # Classe de base avec Id et égalité
│   │   ├── AuditableEntity.cs   # Ajoute CreatedAt et UpdatedAt
│   │   ├── AggregateRoot.cs     # Frontière d'agrégat + Domain Events
│   │   ├── ValueObject.cs       # Égalité par valeur, immutabilité
│   │   └── IDomainEvent.cs      # Marqueur pour les événements métier
│   ├── Users/
│   │   └── User.cs              # Entité racine User
│   ├── ValueObjects/
│   │   ├── Email.cs             # Validation et normalisation de l'email
│   │   ├── FullName.cs          # Prénom + nom avec validation
│   │   └── HashedPassword.cs    # Encapsulation du hash du mot de passe
│   └── Errors/
│       ├── EmailAlreadyTakenError.cs
│       ├── InvalidCredentialsError.cs
│       └── NotFoundError.cs
│
├── Backend.Application/         # Orchestration — dépend uniquement de Domain
│   ├── Abstractions/
│   │   ├── IUnitOfWork.cs       # Contrat de persistance atomique
│   │   ├── IUserRepository.cs   # Contrat d'accès aux données User
│   │   └── IPasswordHasher.cs   # Contrat de hachage de mot de passe
│   ├── Users/
│   │   ├── Commands/
│   │   │   └── CreateUser/
│   │   │       ├── CreateUserCommand.cs
│   │   │       ├── CreateUserHandler.cs
│   │   │       └── CreateUserValidator.cs
│   │   ├── Queries/
│   │   │   └── GetUserById/
│   │   │       ├── GetUserByIdQuery.cs
│   │   │       └── GetUserByIdHandler.cs
│   │   └── Dtos/
│   │       └── UserDto.cs
│   └── Auth/
│       ├── Commands/
│       │   ├── Register/
│       │   │   ├── RegisterCommand.cs
│       │   │   └── RegisterHandler.cs
│       │   └── Login/
│       │       ├── LoginCommand.cs
│       │       └── LoginHandler.cs
│       └── DependencyInjection.cs
│
├── Backend.Persistance/         # Implémentation EF Core — dépend de Application
│   ├── AppDbContext.cs          # DbContext + dispatch Domain Events
│   ├── Configurations/
│   │   └── UserConfiguration.cs # Mapping EF Core (OwnsOne pour Value Objects)
│   ├── Repositories/
│   │   └── UserRepository.cs
│   ├── Interceptors/
│   │   └── AuditableInterceptor.cs # Renseigne CreatedAt/UpdatedAt automatiquement
│   └── DependencyInjection.cs
│
├── Backend.Presentation/        # Controllers — dépend de Application
│   ├── Controllers/
│   │   ├── UsersController.cs
│   │   └── AuthController.cs
│   └── Extensions/
│       └── ResultExtensions.cs  # Mapping Result → IActionResult
│
└── Backend.WebAPI/              # Point d'entrée — compose tout
    ├── Program.cs
    └── appsettings.json
```

---

## Concepts clés

### Clean Architecture

La Clean Architecture organise le code en couches concentriques. Chaque couche a une responsabilité unique et ne connaît que les couches intérieures.

**Règle de dépendance** : le code source ne peut pointer que vers l'intérieur. Le Domain ne dépend de rien. L'Application dépend du Domain. L'Infrastructure et la Presentation dépendent de l'Application.

```csharp
// Le Domain définit une interface
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

// L'Infrastructure l'implémente
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    // ...
}
```

---

### Domain-Driven Design

Le DDD est une approche de conception qui place la logique métier au centre. Les règles métier vivent dans le Domain, pas dans les services ou les controllers.

#### Entity

Une entité est définie par son **identité**. Deux instances avec le même `Id` représentent le même objet, même si leurs propriétés diffèrent.

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
        => obj is Entity other && other.GetType() == GetType() && Id == other.Id;
}
```

#### AuditableEntity

Étend `Entity` avec `CreatedAt` et `UpdatedAt`, renseignés automatiquement par un intercepteur EF Core.

```csharp
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
}
```

---

### Value Objects

Un Value Object est défini par sa **valeur**, pas par une identité. Deux instances avec les mêmes données sont identiques. Il est **immutable** — on le remplace, on ne le modifie pas.

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; private set; } = default!;

    private Email() { }
    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<Email>("Email can't be empty.");
        if (!value.Contains('@'))
            return Result.Fail<Email>("Invalid format.");

        return Result.Ok(new Email(value.ToLowerInvariant().Trim()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Value Objects utilisés :**
- `Email` — validation et normalisation
- `HashedPassword` — encapsulation du hash BCrypt
- `FullName` — prénom + nom avec validation

---

### AggregateRoot

L'`AggregateRoot` est la seule porte d'entrée vers un groupe d'entités liées. Toutes les modifications passent par lui, garantissant la cohérence du Domain. Il est aussi propriétaire des Domain Events.

```csharp
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
```

**Règle** : le Repository ne sauvegarde que l'AggregateRoot, jamais ses entités internes directement.

---

### Domain Events

Les Domain Events permettent de réagir à des changements métier de façon découplée. L'entité annonce ce qui s'est passé, des handlers indépendants réagissent.

```csharp
// L'event est levé dans l'entité
public static Result<User> Create(...)
{
    var user = new User { ... };
    user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
    return Result.Ok(user);
}

// Un handler réagit
public sealed class SendWelcomeEmailHandler
    : INotificationHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken ct)
    {
        // Envoie un email de bienvenue
    }
}
```

Les events sont stockés dans l'agrégat et dispatchés via MediatR **après** le `SaveChanges()` — garantissant que la base est à jour avant toute réaction.

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var result = await base.SaveChangesAsync(ct); // 1. Persiste
    await DispatchDomainEventsAsync(ct);           // 2. Dispatch les events
    return result;
}
```

---

### CQRS avec MediatR

**CQRS** (Command Query Responsibility Segregation) sépare les opérations d'écriture (Commands) des opérations de lecture (Queries).

**MediatR** implémente le pattern Mediator — le Controller envoie un message sans savoir qui le traite.

```
Command  →  modifie l'état   →  CreateUserCommand, RegisterCommand
Query    →  lit l'état       →  GetUserByIdQuery
```

```csharp
// Command
public sealed record CreateUserCommand(
    string Username,
    string Email,
    string Password) : IRequest<Result<UserDto>>;

// Handler
public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        CreateUserCommand cmd, CancellationToken ct)
    {
        // Logique métier
    }
}

// Controller — ne connaît pas le handler
public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd)
    => (await _sender.Send(cmd)).ToActionResult(dto => CreatedAtAction(...));
```

#### Pipeline de Behaviors

MediatR permet d'intercepter toutes les commandes via des `IPipelineBehavior` — validation, logging, gestion des transactions — écrits une seule fois et appliqués automatiquement.

---

### Unit of Work

Le pattern Unit of Work regroupe plusieurs opérations en une seule transaction atomique. Si l'une échoue, tout est annulé.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

`AppDbContext` implémente cette interface. Les repositories ne font jamais `SaveChanges()` eux-mêmes — ils ajoutent seulement les entités au contexte EF Core. Un seul `SaveChangesAsync()` en fin de handler commite tout.

```csharp
await _users.AddAsync(user, ct);     // track dans EF
await _profiles.AddAsync(profile, ct); // track dans EF
await _uow.SaveChangesAsync(ct);     // un seul commit pour les deux
```

---

### FluentResults

FluentResults remplace les exceptions pour les erreurs métier **prévisibles**. Le contrat d'une méthode est explicite dans sa signature de retour.

```csharp
// Le type de retour dit tout
public async Task<Result<UserDto>> Handle(CreateUserCommand cmd, CancellationToken ct)
{
    if (await _users.GetByEmailAsync(cmd.Email, ct) is not null)
        return Result.Fail<UserDto>(new EmailAlreadyTakenError(cmd.Email)); // 409

    var userResult = User.Create(...);
    if (userResult.IsFailed)
        return userResult; // 422

    return Result.Ok(UserDto.FromDomain(userResult.Value)); // 201
}
```

Les erreurs typées sont mappées vers les codes HTTP dans `ResultExtensions.ToActionResult()` :

| Erreur | HTTP |
|--------|------|
| `EmailAlreadyTakenError` | 409 Conflict |
| `NotFoundError` | 404 Not Found |
| `InvalidCredentialsError` | 401 Unauthorized |
| `ValidationError` | 422 Unprocessable Entity |

---

### Authentification par cookie

L'authentification utilise les cookies de session ASP.NET Core — sans ASP.NET Identity. Les claims de l'utilisateur (id, email, username) sont stockés dans le cookie chiffré.

```
POST /api/auth/register  →  crée le compte + connecte automatiquement
POST /api/auth/login     →  vérifie les credentials + crée le cookie
POST /api/auth/logout    →  supprime le cookie
GET  /api/auth/me        →  retourne l'utilisateur connecté (requiert [Authorize])
```

Les routes protégées utilisent `[Authorize]` :

```csharp
[HttpGet("{id:guid}")]
[Authorize]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct) { ... }
```

**Configuration CORS requise** pour les frontends sur un domaine différent :
```javascript
// Côté frontend — obligatoire pour envoyer les cookies
fetch('/api/auth/login', {
    credentials: 'include',
    // ...
});
```

---

## Technologies

| Package | Usage |
|--------|-------|
| `MediatR` | CQRS + Pipeline de behaviors |
| `FluentResults` | Gestion des erreurs sans exceptions |
| `FluentValidation` | Validation des commandes |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | ORM + accès PostgreSQL |
| `BCrypt.Net-Next` | Hachage des mots de passe |
| `Microsoft.AspNetCore.Authentication.Cookies` | Authentification par cookie |

---

## Démarrage rapide

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

### 1. Cloner le repo

```bash
git clone https://github.com/TheWincher/BackendTemplate.git
cd BackendTemplate
```

### 2. Démarrer la base de données

```bash
docker compose up -d
```

### 3. Appliquer les migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/Backend.Persistance/Backend.Persistance.csproj \
  --startup-project src/Backend.WebAPI/Backend.WebAPI.csproj

dotnet ef database update \
  --project src/Backend.Persistance/Backend.Persistance.csproj \
  --startup-project src/Backend.WebAPI/Backend.WebAPI.csproj
```

### 4. Lancer le serveur

```bash
dotnet run --project src/Backend.WebAPI
```

L'API est disponible sur `http://localhost:5111`.

---

## Variables d'environnement

| Variable | Description | Exemple |
|----------|-------------|---------|
| `ConnectionStrings__Default` | Chaîne de connexion PostgreSQL | `Host=localhost;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword` |

En développement, ces valeurs sont dans `appsettings.Development.json`. En production, utilise des variables d'environnement ou un secret manager.

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword"
  }
}
```

---

## Contribuer

1. Fork le projet
2. Crée une branche (`git checkout -b feature/ma-feature`)
3. Commit (`git commit -m 'feat: ajoute ma feature'`)
4. Push (`git push origin feature/ma-feature`)
5. Ouvre une Pull Request