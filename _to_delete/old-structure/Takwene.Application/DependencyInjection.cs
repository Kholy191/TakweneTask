using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Takwene.Application.Artists;
using Takwene.Application.Dsps;
using Takwene.Application.Tracks;

namespace Takwene.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IDspService, DspService>();

        // Registers every AbstractValidator in this assembly (run by the API's validation filter).
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
