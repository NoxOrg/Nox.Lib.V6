using Microsoft.CodeAnalysis;
using Nox.Solution;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class ControllerGenerator : BaseGenerator
    {
        internal ControllerGenerator(GeneratorExecutionContext context) 
            : base(context) 
        {
        }

        internal void GenerateController(Entity entity)
        {
            var sb = new StringBuilder();

            var className = $"{entity.Name}Controller";

            AddBaseTypeDefinition(sb,
                className,
                "ControllerBase",
                "Nox",
                isAbstract: false,
                isPartial: false,
                "Microsoft.AspNetCore.Mvc;",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.AspNetCore.Http");

            foreach (var query in entity.Queries)
            {
                AddProperty(query.Name, query.Name, sb, initOnly: true);
            }

            foreach (var command in entity.Commands)
            {
                AddProperty(command.Name, command.Name, sb, initOnly: true);
            }

            // Generate GET request mapping for Queries
            foreach (var query in entity.Queries)
            {
                sb.AppendLine(@"");
                sb.AppendLine($"    public async Task<IActionResult> Get{query.Name}Async({GetParametersString(query.RequestInput, withDefaults: false)})");
                sb.AppendLine(@"    {");
                sb.AppendLine($"        var result = await {query.Name}Query.ExecuteAsync({GetParametersExecuteString(query.RequestInput)});");
                // TODO: Extend to NotFound and other codes
                sb.AppendLine(@"        return Results.Ok(result);");
                sb.AppendLine(@"    }");
                sb.AppendLine(@"");
            }

            // Generate POST request mapping for Command Handlers
            foreach (var command in entity.Commands)
            {
                sb.AppendLine(@"");
                sb.AppendLine($"    public async Task<IActionResult> {command.Name}(Nox.Commands.{command.Name}{NamingConstants.CommandSuffix} command)");
                sb.AppendLine(@"    {");
                sb.AppendLine($"          var result = await {command.Name}CommandHandlerBase>.ExecuteAsync(command);");
                sb.AppendLine(@"          return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);");
                sb.AppendLine(@"    }");
                sb.AppendLine(@"");
            }

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
