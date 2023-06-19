using Microsoft.CodeAnalysis;
using Nox.Solution;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class CommandGenerator : BaseGenerator
    {
        internal CommandGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddCommandHandler(DomainCommand command)
        {
            var sb = new StringBuilder();

            var className = $"{command.Name}CommandHandlerBase";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicCommandHandler",
                "Nox.Commands",
                isAbstract: true,
                isPartial: false,
                    "Nox.Core.Interfaces.Entity.Commands",
                    "Nox.Core.Interfaces.Messaging",
                    "Nox.Events"
                );

            // Add Db Context
            AddDbContextProperty(sb);

            // Add Db Context
            AddNoxMessangerProperty(sb);

            // Add constructor
            AddConstructor(sb, className, new Dictionary<string, string> {
                { NamingConstants.DbContextName, "DbContext" },
                { "INoxMessenger", "Messenger" }
            });

            // Add params
            sb.AppendLine($@"   public abstract Task<INoxCommandResult> ExecuteAsync({command.Name}{NamingConstants.CommandSuffix} command);");

            // Add Events
            if (command.EmitEvents != null)
            {
                foreach (var domainEvent in command.EmitEvents)
                {
                    AddDomainEvent(sb, domainEvent);
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
