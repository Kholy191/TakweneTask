using System.Security.Cryptography;

namespace Takwene.Application.Common;

// Random temporary password that satisfies the Identity password policy.
public static class PasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*?";
    private const string All = Upper + Lower + Digits + Symbols;

    public static string Generate(int length = 16)
    {
        var chars = new List<char>
        {
            Pick(Upper), Pick(Lower), Pick(Digits), Pick(Symbols)
        };

        while (chars.Count < length)
            chars.Add(Pick(All));

        return new string([.. chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))]);
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
