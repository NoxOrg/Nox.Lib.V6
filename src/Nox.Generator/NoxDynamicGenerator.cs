using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace Nox.Generator;

[Generator]
public class NoxDynamicGenerator : ISourceGenerator
{
    private readonly DiagnosticDescriptor NI0000 = new DiagnosticDescriptor("NI0000", "Nox Generator Debug", "{0}", "Debug", DiagnosticSeverity.Info, true);
    //Warnings
    private readonly DiagnosticDescriptor NW0001 = new("NW0001", "No yaml definitions",
        "Nox.Generator will not contribute to your project as no yaml definitions were found", "Design",
        DiagnosticSeverity.Warning, true);
    
    private readonly DiagnosticDescriptor NW0002 = new("NW0002", "AppSettings",
        "DefinitionRootPath value not found in appsettings.json", "Design",
        DiagnosticSeverity.Warning, true);
    //Errors
    private readonly DiagnosticDescriptor NE0001 = new("NE0001", "Duplicate Entity",
        "Duplicate entity detected in yaml configuration: {0}", "Design",
        DiagnosticSeverity.Error, true);

    
    private List<string>? _entityNames;
    
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }
    
    public void Execute(GeneratorExecutionContext context)
    {
        _entityNames = new List<string>();
        context.ReportDiagnostic(Diagnostic.Create(NI0000, null, $"Executing Nox Generator"));
        var assemblyName = context.Compilation.AssemblyName;
        var mainSyntaxTree = context.Compilation.SyntaxTrees
            .FirstOrDefault(x => x.FilePath.EndsWith("Program.cs"));
        
        if (mainSyntaxTree == null) return;
        
        var programPath = Path.GetDirectoryName(mainSyntaxTree.FilePath);
        var designRootFullPath = Path.GetFullPath(programPath!);
        
        var json = Path.Combine(programPath!, "appsettings.json");
        if (File.Exists(json))
        {
            var config = JObject.Parse(File.ReadAllText(json));
            var designRoot = config["Nox"]?["DefinitionRootPath"]?.ToString();
            if (string.IsNullOrEmpty(designRoot))
            {
                context.ReportDiagnostic(Diagnostic.Create(NW0002, null));
            }
            else
            {
                designRootFullPath = Path.GetFullPath(Path.Combine(programPath!, designRoot));    
            }
        }  
        
        var deserializer = new DeserializerBuilder().Build();

        var entities = Directory
            .EnumerateFiles(designRootFullPath, "*.entity.nox.yaml", SearchOption.AllDirectories)
            .Select(f => deserializer.Deserialize(new StringReader(File.ReadAllText(f))))
            .ToList();

        if (!entities.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(NW0001, null));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(NI0000, null, $"{entities.Count} entities found"));
            
            foreach (Dictionary<object, object>? entity in entities)
            {
                if (AddEntity(context, assemblyName!, entity!))
                {
                    AddDomainEvent(context, assemblyName!, GeneratorEventTypeEnum.Created, entity!);
                    AddDomainEvent(context, assemblyName!, GeneratorEventTypeEnum.Updated, entity!);
                    AddDomainEvent(context, assemblyName!, GeneratorEventTypeEnum.Deleted, entity!);    
                }
            }    
        }
    }

    private string ClassDataType(string type)
    {
        var propType = type.ToLower() ?? "string";

        return propType switch
        {
            "string" => "string",
            "varchar" => "string",
            "nvarchar" => "string",
            "char" => "string",
            "guid" => "Guid",
            "url" => "string",
            "email" => "string",
            "date" => "DateTime",
            "time" => "DateTime",
            "timespan" => "TimeSpan",
            "datetime" => "DateTimeOffset",
            "bool" => "bool",
            "boolean" => "bool",
            "object" => "object",
            "int" => "int",
            "uint" => "uint",
            "tinyint" => "int",
            "bigint" => "long",
            "money" => "decimal",
            "smallmoney" => "decimal",
            "decimal" => "decimal",
            "real" => "single",
            "float" => "single",
            "bigreal" => "double",
            "bigfloat" => "double",
            _ => "string"
        };

    }

    private bool AddEntity(GeneratorExecutionContext context, string assemblyName, Dictionary<object, object>? entity)
    {
        var entityName = entity!["Name"].ToString();
        context.ReportDiagnostic(Diagnostic.Create(NI0000, null, $"Adding Entity class: {entityName} from assembly {assemblyName}"));
        if (_entityNames!.Any(n => n == entityName))
        {
            context.ReportDiagnostic(Diagnostic.Create(NE0001, null, entityName));
            return false;
        }
        _entityNames!.Add(entityName);
        var sb = new StringBuilder();

        sb.AppendLine(@"// autogenerated");
        sb.AppendLine(@"using Nox.Core.Interfaces.Entity;");
        sb.AppendLine(@"");
        sb.AppendLine(@"namespace Nox;");
        sb.AppendLine(@"");
        sb.AppendLine($@"public class {entityName} : IDynamicEntity");
        sb.AppendLine(@"{");

        var attributes = (List<object>)entity["Attributes"];
        foreach (Dictionary<object, object> attr in attributes)
        {
            sb.AppendLine($@"   public {ClassDataType((string)attr["Type"])} {attr["Name"]} {{get; set;}}");
        }

        sb.AppendLine(@"}");

        var hintName = $"{entityName}.g.cs";
        var source = SourceText.From(sb.ToString(), Encoding.UTF8);

        context.AddSource(hintName, source);
        return true;
    }

    private void AddDomainEvent(GeneratorExecutionContext context, string assemblyName, GeneratorEventTypeEnum generatorEventType, Dictionary<object, object>? entity)
    {
        var sb = new StringBuilder();

        var eventTypeName = "";
        switch (generatorEventType)
        {
            case GeneratorEventTypeEnum.Created:
                eventTypeName = "Create";
                break;
            case GeneratorEventTypeEnum.Updated:
                eventTypeName = "Update";
                break;
            case GeneratorEventTypeEnum.Deleted:
                eventTypeName = "Delete";
                break;
        }

        var className = $"{entity!["Name"]}{eventTypeName}dDomainEvent";

        sb.AppendLine($@"// autogenerated");
        sb.AppendLine($@"using Nox.Messaging.Events;");
        sb.AppendLine($@"");
        sb.AppendLine($@"namespace Nox;");
        sb.AppendLine($@"");
        sb.AppendLine($@"public partial class {className} : Nox{eventTypeName}Event<{entity["Name"]}>");
        sb.AppendLine($@"{{");
        sb.AppendLine($@"}}");

        var hintName = $"{className}.g.cs";
        var source = SourceText.From(sb.ToString(), Encoding.UTF8);

        context.AddSource(hintName, source);
    }
}