using Microsoft.CodeAnalysis;
using Nox.Solution;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class EntityGenerator : BaseGenerator
    {
        internal EntityGenerator(GeneratorExecutionContext context)
            : base(context) { }

        internal bool AddEntity(Entity entity)
        {
            GenerateEntity(entity);

            if (entity.Queries != null)
            {
                GenerateQueries(entity.Queries);
            }

            if (entity.Events != null)
            {
                GenerateDomainEvents(entity.Events);
            }

            // Commands - may depend on events
            if (entity.Commands != null)
            {
                GenerateCommands(entity.Commands);
            }

            return true;
        }

        private void GenerateDomainEvents(IReadOnlyList<NoxSimpleTypeDefinition> events)
        {
            var eventsGenerator = new EventsGenerator(Context);

            foreach (var domainEvent in events)
            {
                eventsGenerator.AddDomainEvent(domainEvent.Name, domainEvent.EntityTypeOptions.Entity);
            }
        }

        private void GenerateEntity(Entity entity)
        {
            var sb = new StringBuilder();

            AddBaseTypeDefinition(sb,
                entity.Name,
                "IDynamicEntity",
                "Nox",
                isAbstract: false,
                isPartial: true,
                "Nox.Core.Interfaces.Entity");

            if (entity.Keys == null || !entity.Keys.Any())
            {
                Context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NW0001, null, $"Entity {entity.Name} must have a primary key defined."));
                return;
            }

            // Key
            AddPrimaryKey(entity.Keys, sb);

            // Attributes
            if (entity.Attributes != null)
            {
                AddAttributes(entity.Attributes, sb);
            }

            // Relationships
            if (entity.Relationships != null)
            {
                AddRelationships(entity.Relationships, sb);
            }

            // Owned Relationships - define Aggregate Root boundaries
            if (entity.OwnedRelationships != null)
            {
                AddRelationships(entity.OwnedRelationships, sb);
            }

            sb.AppendLine(@"}");

            GenerateFile(sb, entity.Name);
        }

        private void AddPrimaryKey(IEnumerable<NoxSimpleTypeDefinition> keys, StringBuilder sb)
        {
            foreach (var key in keys)
            {
                if (key.Type == NoxType.entity)
                {
                    AddProperty(key.EntityTypeOptions.Entity, key.Name, sb);
                }
                else
                {
                    AddSimpleProperty(key.Type, key.Name, true, sb);
                }
            }
        }

        private void GenerateCommands(IEnumerable<DomainCommand> commands)
        {
            var commandGenerator = new CommandGenerator(Context);

            foreach (var command in commands)
            {
                commandGenerator.AddCommandHandler(command);
            }
        }

        private void GenerateQueries(IEnumerable<DomainQuery> queries)
        {
            var queryGenerator = new QueryGenerator(Context);

            foreach (var query in queries)
            {
                queryGenerator.AddQuery(query);
            }
        }
    }
}
