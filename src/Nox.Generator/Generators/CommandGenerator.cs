using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class CommandGenerator : BaseGenerator
    {
        internal CommandGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddCommandHandler(Dictionary<object, object> command, Dictionary<string, string> eventsProviders)
        {
            var sb = new StringBuilder();

            var className = $"{command["name"]}CommandHandlerBase";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicCommandHandler",
                "Nox.Commands",
                isAbstract: true,
                new[] 
                {
                    "Nox.Core.Interfaces.Entity.Commands",
                    "Nox.Core.Interfaces.Messaging",
                    "Nox.Dto",
                    "Nox.Events"
                });

            // Add Db Context
            AddDbContextProperty(sb);

            // Add Db Context
            AddNoxMessangerProperty(sb);
                        
            // Add constructor
            AddConstructor(sb, className, new Dictionary<string, string> {
                { "NoxDbContext", "DbContext" },
                { "INoxMessenger", "Messenger" }
            });

            // Add params (which can be DTO)
            string parameters = GetParametersString(command["parameters"]);

            // Add DTO or params
            sb.AppendLine($@"   public abstract Task<INoxCommandResult> ExecuteAsync({parameters});");

            // Add Events
            command.TryGetValue("events", out var events);
            if (events != null)
            {
                foreach (var domainEvent in (List<object>)events)
                {
                    AddDomainEvent(sb, (string)domainEvent, eventsProviders);
                }
            }

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }

        private static void AddDomainEvent(StringBuilder sb, string eventName, Dictionary<string, string> eventsProviders)
        {
            sb.AppendLine($@"");
            sb.AppendLine($@"   public async Task Send{eventName}DomainEventAsync({eventName}DomainEvent domainEvent)");
            sb.AppendLine($@"   {{");
            sb.AppendLine($@"       await Messenger.SendMessageAsync(new string[] {{ ""{eventsProviders[eventName]}"" }}, domainEvent);");
            sb.AppendLine($@"   }}");
        }
    }
}
