using System.Reflection;
using Backend.Application.Abstractions;
using Backend.Domain.Primitives;
using Backend.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Persistence;

public class AppDbContext(DbContextOptions options, IPublisher publisher) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users {get; set;}
    private readonly IPublisher _publisher = publisher;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    base.OnModelCreating(modelBuilder);
}

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync(ct);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var events = aggregates
            .SelectMany(e => e.DomainEvents)
            .ToList();

        aggregates.ForEach(e => e.ClearDomainEvents());

        foreach (var ev in events)
            await _publisher.Publish(ev, ct);
    }
}