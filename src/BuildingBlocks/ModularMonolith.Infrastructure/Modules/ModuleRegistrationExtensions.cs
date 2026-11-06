using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Infrastructure.Modules;

public static class ModuleRegistrationExtensions
{
    public static IServiceCollection AddModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : class, IModule, new()
    {
        var module = new TModule();

        if (!module.IsEnabled(configuration))
        {
            return services;
        }

        services.AddSingleton<IModule>(module);
        module.RegisterServices(services, configuration);

        return services;
    }

    public static WebApplication MapModules(this WebApplication app)
    {
        foreach (var module in app.Services.GetServices<IModule>())
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}
