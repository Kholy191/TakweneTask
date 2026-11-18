using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Takwene.Infrastructure.Persistence.Seeding;

// Seeds a table from Persistence/Seeding/Data/{fileName}, only when the table is empty.
internal static class JsonDataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedAsync<T>(AppDbContext context, string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        if (await context.Set<T>().AnyAsync(cancellationToken))
            return;

        var path = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeding", "Data", fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        await using var stream = File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken);

        if (items is { Count: > 0 })
        {
            context.Set<T>().AddRange(items);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
