using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Infrastructure.Persistence;

public sealed record ModuleDbContextRegistration(Type ContextType);

public static class ModuleDbContextRegistrationExtensions
{
    public static IServiceCollection AddModuleDbContext<TContext>(this IServiceCollection services)
        where TContext : ModuleDbContext
    {
        services.AddSingleton(new ModuleDbContextRegistration(typeof(TContext)));

        return services;
    }
}
