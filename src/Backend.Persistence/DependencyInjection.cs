using Backend.Application.Abstractions;
using Backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var currentAssembly = typeof (DependencyInjection).Assembly;

        var connectionString = configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException(
            "connection string 'Default' not found. Please ensure it is defined in the configuration.");

            Console.WriteLine($"Using connection string: {connectionString}");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
