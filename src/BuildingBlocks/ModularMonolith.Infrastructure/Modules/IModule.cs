using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Infrastructure.Modules;

public interface IModule
{
    string Name { get; }

    bool IsEnabled(IConfiguration configuration);

    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
