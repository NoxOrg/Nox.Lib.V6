using System.Text.RegularExpressions;
using Serilog;

namespace Nox.Core.Helpers.TextMacros;

internal sealed class TextEnvironmentVariableMacroParser : ITextMacroParser
{
    private readonly IEnvironmentProvider _environmentProvider;

    public TextEnvironmentVariableMacroParser(IEnvironmentProvider environmentProvider)
    {
        _environmentProvider = environmentProvider;
    }
    private readonly Regex _regex = new Regex(@"\$\{env:\S+\}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(10000));

    public string Parse(string text)
    {
        var parsed = text;
        MatchCollection? matched = default;
        try
        {
            matched = _regex.Matches(text);
        }
        catch (RegexMatchTimeoutException e)
        {
            Log.Error(e,"Timeout expanding Environment Variables");
            return text;
        }

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