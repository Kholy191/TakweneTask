using System.Text.RegularExpressions;

namespace Takwene.Domain.Rules;

// ISRC: country (2 letters) + registrant (3 alphanumeric) + year (2 digits) + designation (5 digits).
public static partial class Isrc
{
    public static string Normalize(string value) =>
        value.Replace("-", string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();

    public static bool IsValid(string normalizedValue) => IsrcRegex().IsMatch(normalizedValue);

    [GeneratedRegex("^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$")]
    private static partial Regex IsrcRegex();
}
