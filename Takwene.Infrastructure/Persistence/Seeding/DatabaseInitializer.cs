using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Takwene.Domain.Entities;

namespace Takwene.Infrastructure.Persistence.Seeding;

// Applies pending migrations and seeds data on startup.
public static class DatabaseInitializer
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<AppDbContext>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));

        await MigrateAsync(context, logger, cancellationToken);

        // Order matters: users before artists (UserId), artists and DSPs before tracks and distributions.
        await ActivatorUtilities.CreateInstance<IdentitySeeder>(provider).SeedAsync(cancellationToken);
        await JsonDataSeeder.SeedAsync<Artist>(context, "artists.json", cancellationToken);
        await JsonDataSeeder.SeedAsync<Dsp>(context, "dsps.json", cancellationToken);
        await JsonDataSeeder.SeedAsync<Track>(context, "tracks.json", cancellationToken);
        await JsonDataSeeder.SeedAsync<TrackDistribution>(context, "distributions.json", cancellationToken);

        logger.LogInformation("Database seeding completed");
    }

    // Retries because PostgreSQL may still be starting (e.g. right after docker compose up).
    private static async Task MigrateAsync(AppDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied");
                return;
            }
            catch (NpgsqlException ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning(ex, "Database not reachable (attempt {Attempt}/{MaxAttempts}); retrying", attempt, MaxAttempts);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }
}
