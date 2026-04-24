using Microsoft.Extensions.DependencyInjection;

namespace Backend.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        var currentAssembly = typeof (DependencyInjection).Assembly;
        
        return services;
    }
}
