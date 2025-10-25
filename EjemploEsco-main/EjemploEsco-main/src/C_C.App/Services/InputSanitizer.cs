using System.Text.RegularExpressions;

namespace C_C.App.Services;

public class InputSanitizer
{
    private static readonly Regex ControlChars = new("[\\x00-\\x1F]+", RegexOptions.Compiled);

    public string Sanitize(string value)
    {
        var normalized = value.Trim();
        normalized = ControlChars.Replace(normalized, string.Empty);
        return normalized;
    }
}
