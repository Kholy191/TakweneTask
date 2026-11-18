namespace Takwene.Application.Common;

// Permission claims carried in the JWT; each one is also an authorization policy name.
public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Artists
    {
        public const string Read = "artists:read";
        public const string Create = "artists:create";
    }

    public static class Tracks
    {
        public const string Read = "tracks:read";
        public const string Create = "tracks:create";
        public const string Distribute = "tracks:distribute";
        public const string UpdateStatus = "tracks:update_status";
    }

    public static class Distributions
    {
        public const string UpdateStatus = "distributions:update_status";
    }

    public static class Dsps
    {
        public const string Read = "dsps:read";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Artists.Read, Artists.Create,
        Tracks.Read, Tracks.Create, Tracks.Distribute, Tracks.UpdateStatus,
        Distributions.UpdateStatus,
        Dsps.Read
    ];

    // Permissions granted to each role (seeded as role claims).
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ByRole = new Dictionary<string, IReadOnlyList<string>>
    {
        [Roles.Admin] = All,
        [Roles.Artist] = [Tracks.Read, Tracks.Create, Tracks.Distribute, Tracks.UpdateStatus, Dsps.Read]
    };
}
