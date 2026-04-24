using Backend.Application.Abstractions;
using Backend.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var currentAssembly = typeof (DependencyInjection).Assembly;

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        
        return services;
    }
}
