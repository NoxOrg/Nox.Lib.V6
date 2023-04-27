using System.Text.RegularExpressions;
using Nox.Core.Exceptions;

namespace Nox.Core.Helpers;

public static class YamlHelper
{
    private static readonly Regex _referenceRegex = new(@"\$ref\S*:\s*(?<variable>[\w:\.\/\\]+\b[\w\-\.\/]+)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

    /// <summary>
    /// Resolve $ref &lt;path&gt; tags in a yaml source yaml file. <br/>
    /// Loads the source yaml file, resolves the $ref's and replaces them with the yaml from the files specified in &lt;path&gt;<br/>
    /// Note: This call is recursive, all $refs in the yaml hierarchy will be resolved.
    /// Note: child nodes are added at the same indentation as the $ref tag.
    /// </summary>
    /// <param name="path">Full or relative path to the source yaml file.</param>
    /// <returns></returns>
    public static string ResolveYamlReferences(string path)
    {
        var sourceFullPath = Path.GetFullPath(path);
        if (!File.Exists(sourceFullPath)) throw new NoxYamlException($"Yaml file {path} does not exist!");
        var sourcePath = Path.GetDirectoryName(sourceFullPath);
        
        var sourceLines = File.ReadAllLines(path);
        var outputLines = ResolveYamlReferences(sourceLines.ToList(), sourcePath!).Result;

        return string.Join('\n', outputLines.ToArray());
    }
    
    /// <summary>
    /// Resolve $ref &lt;path&gt; tags in a yaml source yaml file. <br/>
    /// Loads the source yaml file, resolves the $ref's and replaces them with the yaml from the files specified in &lt;path&gt;<br/>
    /// Note: This call is recursive, all $refs in the yaml hierarchy will be resolved.
    /// Note: child nodes are added at the same indentation as the $ref tag.
    /// </summary>
    /// <param name="path">Full or relative path to the source yaml file.</param>
    /// <returns></returns>
    public static async Task<string> ResolveYamlReferencesAsync(string path)
    {
        var sourceFullPath = Path.GetFullPath(path);
        if (!File.Exists(sourceFullPath)) throw new NoxYamlException($"Yaml file {path} does not exist!");
        var sourcePath = Path.GetDirectoryName(sourceFullPath);
        
        var sourceLines = await File.ReadAllLinesAsync(path);
        var outputLines = await ResolveYamlReferences(sourceLines.ToList(), sourcePath!);

        return string.Join('\n', outputLines.ToArray());
    }

    private static async Task<List<string>> ResolveYamlReferences(List<string> sourceLines, string path)
    {
        var outputLines = new List<string>();
        foreach (var sourceLine in sourceLines)
        {
            var match = _referenceRegex.Match(sourceLine);
            if (match.Success)
            {
                var prefix = sourceLine.Substring(0, match.Index);
                var childPath = match.Groups[1].Value;
                if (!Path.IsPathRooted(childPath)) childPath = Path.Combine(path!, childPath);
                if (!File.Exists(childPath)) throw new NoxYamlException($"Referenced yaml file does not exist for reference: {match.Groups[1].Value}");
                var childLines = await File.ReadAllLinesAsync(childPath);
                foreach (var childLine in childLines)
                {
                    outputLines.Add(prefix + childLine);
                }
            }
            else
            {
                outputLines.Add(sourceLine);
            }
        }

        if (outputLines.Any(ol => ol.Contains("$ref:")))
        {
            outputLines = await ResolveYamlReferences(outputLines, path);    
        }
        
        return outputLines;
    }

}
