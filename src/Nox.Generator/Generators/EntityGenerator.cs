using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class EntityGenerator : BaseGenerator
    {
        internal List<string> AggregateRoots { get; set; } = new List<string>();

        internal EntityGenerator(GeneratorExecutionContext context) : base(context) { }

        private readonly List<string> _entityNames = new();

        internal bool AddEntity(string assemblyName, Dictionary<object, object> entity)
        {
            var entityName = entity["name"].ToString();

            Context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"Adding Entity class: {entityName} from assembly {assemblyName}"));

            if (_entityNames.Any(n => n == entityName))
            {
                Context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NE0001, null, entityName));

                return false;
            }

            _entityNames.Add(entityName);

            GenerateEntity(entity, entityName);

            GenerateQueries(entity);

            GenerateCommands(entity);

            GenerateEvents(entity, entityName);

            return true;
        }

        private void GenerateEntity(Dictionary<object, object> entity, string entityName)
        {
            var sb = new StringBuilder();

            bool isAggreagteRoot = GetBooleanValueOrDefault(entity, "isAggregateRoot");

            AddBaseTypeDefinition(sb,
                entityName,
                isAggreagteRoot ? "IDynamicAggregateRoot" : "IDynamicEntity",
                "Nox",
                isAbstract: false,
                new[] { "Nox.Core.Interfaces.Entity" });

            // Attributes
            AddAttributes(entity, sb);

            // Relations
            entity.TryGetValue("relations", out var relations);
            if (relations != null)
            {
                foreach (var attr in ((List<object>)relations).Cast<Dictionary<object, object>>())
                {
                    bool isCollection = bool.Parse((string)attr["isCollection"]);
                    var typeDefinition = isCollection ? $"IList<{attr["entity"]}>" : $"{attr["entity"]}";
                    sb.AppendLine($@"   public {typeDefinition} {attr["name"]} {{ get; set; }}");
                    sb.AppendLine($@"");
                }
            }

            sb.AppendLine(@"}");

            GenerateFile(sb, entityName);

            if (isAggreagteRoot)
            {
                AggregateRoots.Add(entityName);
            }
        }

        private void GenerateEvents(Dictionary<object, object> entity, string entityName)
        {
            var eventsGenerator = new EventsGenerator(Context);

            // Basic CRUD events
            entity.TryGetValue("raiseCrudEvents", out var raiseCrudEvents);

            if (raiseCrudEvents != null)
            {
                var crudEvents = (Dictionary<object, object>)raiseCrudEvents;

                if (GetBooleanValueOrDefault(crudEvents, "create"))
                {
                    eventsGenerator.AddCrudEvent(GeneratorEventType.Created, entityName);
                }

                if (GetBooleanValueOrDefault(crudEvents, "update"))
                {
                    eventsGenerator.AddCrudEvent(GeneratorEventType.Updated, entityName);
                }

                if (GetBooleanValueOrDefault(crudEvents, "delete"))
                {
                    eventsGenerator.AddCrudEvent(GeneratorEventType.Deleted, entityName);
                }
            }

            entity.TryGetValue("events", out var events);
            if (events != null)
            {
                foreach (var domainEvent in ((List<object>)events).Cast<Dictionary<object, object>>())
                {
                    eventsGenerator.AddDomainEvent((string)domainEvent["name"], (string)domainEvent["dto"]);
                }
            }
        }

        private void GenerateCommands(Dictionary<object, object> entity)
        {
            entity.TryGetValue("commands", out var commands);
            if (commands != null)
            {
                var commandGenerator = new CommandGenerator(Context);

                foreach (var command in ((List<object>)commands).Cast<Dictionary<object, object>>())
                {
                    commandGenerator.AddCommandHandler(command);
                }
            }
        }

        private void GenerateQueries(Dictionary<object, object> entity)
        {
            entity.TryGetValue("queries", out var queries);
            if (queries != null)
            {
                var queryGenerator = new QueryGenerator(Context);

                foreach (var query in ((List<object>)queries).Cast<Dictionary<object, object>>())
                {
                    queryGenerator.AddQuery(query);
                }
            }
        }
    }
}
