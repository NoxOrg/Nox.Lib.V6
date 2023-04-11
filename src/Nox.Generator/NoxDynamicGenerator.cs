using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Nox.Generator.Generators;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nox.Generator;

[Generator]
public class NoxDynamicGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        /*
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
        */
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var EntityGenerator = new EntityGenerator(context);

        context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"Executing Nox Generator"));

        var assemblyName = context.Compilation.AssemblyName;

        var mainSyntaxTree = context.Compilation.SyntaxTrees
            .FirstOrDefault(x => x.FilePath.EndsWith("Program.cs"));

        if (mainSyntaxTree == null)
        {
            return;
        }

        var programPath = Path.GetDirectoryName(mainSyntaxTree.FilePath);
        var designRootFullPath = Path.GetFullPath(programPath!);

        var json = Path.Combine(programPath!, "appsettings.json");
        if (File.Exists(json))
        {
            var config = JObject.Parse(File.ReadAllText(json));
            var designRoot = config["Nox"]?["DefinitionRootPath"]?.ToString();
            if (string.IsNullOrEmpty(designRoot))
            {
                context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NW0002, null));
            }
            else
            {
                designRootFullPath = Path.GetFullPath(Path.Combine(programPath!, designRoot));
            }
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var entities = GetConfigurationByType(designRootFullPath, deserializer, "entity");

        if (!entities.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NW0001, null));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"{entities.Count} entities found"));

            foreach (var entity in entities.Cast<Dictionary<object, object>?>())
            {
                EntityGenerator.AddEntity(assemblyName!, entity!);
            }
        }
        
        List<object?> dtos = GetConfigurationByType(designRootFullPath, deserializer, "dto");
        var DtoGenerator = new DtoGenerator(context);
        foreach (var dto in dtos.Cast<Dictionary<object, object>>())
        {
            DtoGenerator.AddDTO(dto);
        }

        List<object?> commands = GetConfigurationByType(designRootFullPath, deserializer, "command");
        var commandGenerator = new CommandGenerator(context);
        foreach (var command in commands.Cast<Dictionary<object, object>>())
        {
            commandGenerator.AddCommand(command);
        }

        var DbContextGenerator = new DbContextGenerator(context);
        DbContextGenerator.AddDbContext(EntityGenerator.AggregateRoots.ToArray(), EntityGenerator.CompositeKeys.ToArray());
    }

    private static List<object?> GetConfigurationByType(string designRootFullPath, IDeserializer deserializer, string configType)
    {
        return Directory
            .EnumerateFiles(designRootFullPath, $"*.{configType}.nox.yaml", SearchOption.AllDirectories)
            .Select(f =>
            {
                using var reader = new StringReader(File.ReadAllText(f));
                return deserializer.Deserialize(reader);
            })
            .ToList();
    }
}