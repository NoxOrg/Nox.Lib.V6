using System.Text.RegularExpressions;

namespace Nox.Core.Extensions;

public static class YamlExtensions
{
    private static readonly Regex _referenceRegex = new(@"\$ref\s*(?<variable>[\.\/]+\b[\w\.\/]+)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static string ResolveYamlReferences(this string inputYaml)
    {
        var result = "";
        var match = _referenceRegex.Match(inputYaml);
        if (match.Success)
        {
            //Find the variable value
            var path = match.Groups[2].Value;
        }
        return result;
    }
}