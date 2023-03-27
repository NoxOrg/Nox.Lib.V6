using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class CommandGenerator : BaseGenerator
    {
        internal CommandGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddCommandHandler(Dictionary<object, object> command)
        {
            var sb = new StringBuilder();

            var className = $"{command["name"]}CommandHandlerBase";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicCommandHandler",
                "Nox.Commands",
                isAbstract: true,
                new[] { "Nox.Core.Interfaces.Entity.Commands", "Nox.Events", "Nox.Dto", "Nox.Core.Interfaces.Messaging" });

            // Add Db Context
            AddDbContextProperty(sb);

            // Add Db Context
            AddNoxMessangerProperty(sb);

            // Add Events
            command.TryGetValue("events", out var events);
            if (events != null)
            {
                foreach (var domainEvent in ((List<object>)events).Cast<Dictionary<object, object>>())
                {
                    AddDomainEvent(sb, (string)domainEvent["name"]);
                }
            }

            // Add constructor
            AddConstructor(sb, className);

            // Add params (which can be DTO)
            string parameters = GetParametersString(command["parameters"]);

            // Add DTO or params
            sb.AppendLine($@"   public abstract Task<INoxCommandResult> ExecuteAsync({parameters});");

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
