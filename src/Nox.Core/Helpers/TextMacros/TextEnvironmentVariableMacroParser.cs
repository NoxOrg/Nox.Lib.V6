using System.Text.RegularExpressions;

namespace Nox.Core.Helpers.TextMacros;

internal sealed class TextEnvironmentVariableMacroParser : ITextMacroParser
{
    private readonly IEnvironmentProvider _environmentProvider;

    public TextEnvironmentVariableMacroParser(IEnvironmentProvider environmentProvider)
    {
        _environmentProvider = environmentProvider;
    }
    private readonly Regex _regex = new Regex(@"\$\{env:\S+\}");

    public string Parse(string text)
    {
        var parsed = text;
        MatchCollection matched = _regex.Matches(text);
        for (int count = 0; count < matched.Count; count++)
        {
            var match = matched[count];
            var variableName = match.Value.Substring(6, match.Value.Length - 1 - 6);
            var environmentValue = _environmentProvider.GetEnvironmentVariable(variableName);

            if (environmentValue != null)
            {
                parsed = parsed.Replace(match.Value, environmentValue);
            }
        }

        return parsed;
    }
}