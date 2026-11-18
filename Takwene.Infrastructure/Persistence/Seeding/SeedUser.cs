namespace Takwene.Infrastructure.Persistence.Seeding;

// One entry of users.json; the password is a documented test password.
internal sealed record SeedUser(string Id, string FullName, string Email, string Password, string Role);
