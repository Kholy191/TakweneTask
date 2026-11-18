using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Takwene.Infrastructure.Persistence.Seeding;

// Seeds a table from Persistence/Seeding/Data/{fileName}, only when the table is empty.
internal static class JsonDataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync<T>(AppDbContext context, string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        if (await context.Set<T>().AnyAsync(cancellationToken))
            return;

        var items = await ReadAsync<T>(fileName, cancellationToken);
        if (items.Count == 0)
            return;

        context.Set<T>().AddRange(items);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task<List<T>> ReadAsync<T>(string fileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeding", "Data", fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken) ?? [];
    }
}
