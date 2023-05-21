using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class WebHelperGenerator : BaseGenerator
    {
        internal WebHelperGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddQueries(IEnumerable<Dictionary<object, object>> queries, IEnumerable<Dictionary<object, object>> commands)
        {
            var sb = new StringBuilder();

            var className = "ApiHelper";

            AddBaseTypeDefinition(sb,
                className,
                null,
                "Nox",
                isAbstract: false,
                isPartial: false,
                "Microsoft.AspNetCore.Builder",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.AspNetCore.Http");

            // Generate GET request mapping for Queries
            foreach (var query in queries)
            {
                sb.AppendLine(@"");
                sb.AppendLine($"    public static void {query["name"]}(WebApplication app, string path)");
                sb.AppendLine(@"    {");
                sb.AppendLine(@"        app.MapGet(path,");
                sb.AppendLine($"            async ({GetParametersString(query["parameters"], withDefaults: false)}) =>");
                sb.AppendLine(@"            {");
                sb.AppendLine(@"                using var scope = app.Services.CreateScope();");
                sb.AppendLine($"                var result = await scope.ServiceProvider.GetRequiredService<Nox.Queries.{query["name"]}Query>().ExecuteAsync({GetParametersExecuteString(query["parameters"])});");
                // TODO: Extend to NotFound and other codes
                sb.AppendLine(@"                return Results.Ok(result);");
                sb.AppendLine(@"            });");
                sb.AppendLine(@"    }");
                sb.AppendLine(@"");
            }

            // Generate POST request mapping for Command Handlers
            foreach (var command in commands)
            {
                sb.AppendLine(@"");
                sb.AppendLine($"    public static void {command["name"]}(WebApplication app, string path)");
                sb.AppendLine(@"    {");
                sb.AppendLine(@"        app.MapPost(path,");
                sb.AppendLine($"            async (Nox.Commands.{command["name"]}{NamingConstants.CommandSuffix} command) =>");
                sb.AppendLine(@"            {");
                sb.AppendLine(@"                using var scope = app.Services.CreateScope();");
                sb.AppendLine($"                var result = await scope.ServiceProvider.GetRequiredService<Nox.Commands.{command["name"]}CommandHandlerBase>().ExecuteAsync(command);");
                sb.AppendLine(@"                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);");
                sb.AppendLine(@"            });");
                sb.AppendLine(@"    }");
                sb.AppendLine(@"");
            }

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
