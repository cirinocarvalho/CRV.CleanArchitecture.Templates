using System.Globalization;
using System.Text.RegularExpressions;

namespace StarterApp.Application.Common;

/// <summary>
/// Utility class for regex-based validations.
/// </summary>
public static partial class RegexUtilities
{
    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex PhoneCleanupRegex();

    /// <summary>
    /// Strips non-alphanumeric characters from a phone number string.
    /// </summary>
    public static string CleanPhone(string input) => PhoneCleanupRegex().Replace(input, string.Empty);

    [GeneratedRegex("(@)(.+)$")]
    private static partial Regex DomainPartRegex();

    [GeneratedRegex(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 250)]
    private static partial Regex EmailValidationRegex();

    /// <summary>
    /// Validates an email address format, including IDN domain support.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain part using IDN mapping.
            email = DomainPartRegex().Replace(email, match =>
            {
                var idn = new IdnMapping();
                var domainName = idn.GetAscii(match.Groups[2].Value);
                return match.Groups[1].Value + domainName;
            });
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return EmailValidationRegex().IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
