using System.Text.RegularExpressions;

namespace Gdc.Infrastructure.Auth;

internal static partial class PasswordPolicy
{
    public static bool IsCompliant(string password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= 8
        && LetterRegex().IsMatch(password)
        && DigitRegex().IsMatch(password);

    public const string InvalidMessage =
        "La contraseña debe tener al menos 8 caracteres, una letra y un número.";

    [GeneratedRegex(@"[A-Za-z]")]
    private static partial Regex LetterRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();
}
