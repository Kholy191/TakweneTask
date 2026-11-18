using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Takwene.Application.Interfaces;
using Takwene.Application.Services;

namespace Takwene.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<ITrackDistributionService, TrackDistributionService>();
        services.AddScoped<IDspService, DspService>();

        // Registers every AbstractValidator in this assembly (run by the API's validation filter).
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
