using Microsoft.CodeAnalysis;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class EventsGenerator : BaseGenerator
    {
        internal EventsGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddDomainEvent(string eventName, string typeName)
        {
            var sb = new StringBuilder();

            var className = $"{eventName}DomainEvent";
            
            AddBaseTypeDefinition(sb,
                className,
                $"Nox{GeneratorEventType.Domain}Event<{typeName}>",
                "Nox.Events",
                isAbstract: false,
                isPartial: false,
                new[] { "Nox.Core.Interfaces.Messaging.Events", "Nox.Dto" });

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }

        internal void AddCrudEvent(GeneratorEventType generatorEventType, string entityName)
        {
            var sb = new StringBuilder();

            var eventTypeName = generatorEventType.ToString();

            var className = $"{entityName}{eventTypeName}Event";
                        
            AddBaseTypeDefinition(sb,
                className,
                $"Nox{eventTypeName}Event<{entityName}>",
                "Nox.Events",
                isAbstract: false,
                isPartial: false,
                new[] { "Nox.Core.Interfaces.Messaging.Events" });

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
