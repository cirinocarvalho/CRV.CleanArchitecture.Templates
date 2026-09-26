namespace StarterApp.Application.Common;

/// <summary>
/// Neutralises caller-controlled text before it is written to a log.
/// Anything that originates in a request (path, method, headers, file names,
/// claims) can contain CR/LF sequences that forge extra log lines or corrupt
/// the JSON log files (CodeQL "Log entries created from user input").
/// </summary>
public static class LogSanitizer
{
    /// <summary>
    /// Maximum length written to the log for a single caller-supplied value.
    /// Long enough for any legitimate path or file name, short enough that a
    /// hostile request cannot bloat the log files.
    /// </summary>
    private const int MaxLength = 512;

    /// <summary>
    /// Strips line breaks and other control characters from <paramref name="value"/>
    /// and truncates it, so the returned string can only ever occupy a single log line.
    /// Never returns null; a null or empty input yields an empty string.
    /// </summary>
    public static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // The two explicit Replace calls are what static analysers recognise as
        // the log-forging sanitizer; the control-character filter below covers
        // the rest (tab, NUL, ANSI escape introducers, ...).
        var text = value.Replace("\r", string.Empty).Replace("\n", string.Empty);

        var chars = new char[Math.Min(text.Length, MaxLength)];
        for (var i = 0; i < chars.Length; i++)
        {
            var c = text[i];
            chars[i] = char.IsControl(c) ? '_' : c;
        }

        return text.Length > MaxLength
            ? new string(chars) + "..."
            : new string(chars);
    }
}
