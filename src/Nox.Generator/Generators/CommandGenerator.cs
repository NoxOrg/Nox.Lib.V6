using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class CommandGenerator : BaseGenerator
    {
        internal CommandGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddCommand(Dictionary<object, object> command)
        {
            var sb = new StringBuilder();

            var className = $"{command["name"]}{NamingConstants.CommandSuffix}";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicDTO",
                "Nox.Commands",
                isAbstract: false,
                isPartial: false,
                "Nox.Core.Interfaces.Entity");

            AddAttributes(command, sb);

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }

        internal void AddCommandHandler(Dictionary<object, object> command)
        {
            var sb = new StringBuilder();

            var className = $"{command["name"]}CommandHandlerBase";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicCommandHandler",
                "Nox.Commands",
                isAbstract: true,
                isPartial: false,
                new[] 
                {
                    "Nox.Core.Interfaces.Entity.Commands",
                    "Nox.Core.Interfaces.Messaging",
                    "Nox.Events"
                });

            // Add Db Context
            AddDbContextProperty(sb);

            // Add Db Context
            AddNoxMessangerProperty(sb);
                        
            // Add constructor
            AddConstructor(sb, className, new Dictionary<string, string> {
                { "NoxDomainDbContext", "DbContext" },
                { "INoxMessenger", "Messenger" }
            });
                        
            // Add params
            sb.AppendLine($@"   public abstract Task<INoxCommandResult> ExecuteAsync({command["name"]}{NamingConstants.CommandSuffix} command);");

            // Add Events
            command.TryGetValue("events", out var events);
            if (events != null)
            {
                foreach (var domainEvent in (List<object>)events)
                {
                    AddDomainEvent(sb, (string)domainEvent);
                }
            }

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }

        private static void AddDomainEvent(StringBuilder sb, string eventName)
        {
            sb.AppendLine($@"");
            sb.AppendLine($@"   public async Task Send{eventName}DomainEventAsync({eventName}DomainEvent domainEvent)");
            sb.AppendLine($@"   {{");
            sb.AppendLine($@"       await Messenger.SendMessageAsync(new string[] {{ ""{NamingConstants.DefaultMessagingProvider}"" }}, domainEvent);");
            sb.AppendLine($@"   }}");
        }
    }
}
