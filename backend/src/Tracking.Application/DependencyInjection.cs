using Microsoft.Extensions.DependencyInjection;
using Tracking.Application.Services;

namespace Tracking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IPilotoService, PilotoService>();
        return services;
    }
}
