using Microsoft.CodeAnalysis;
using Nox.Solution;
using System.Collections.Generic;
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

            GenerateQueries(entity.Queries);

            GenerateDomainEvents(entity.Events);

            // Depends on events
            GenerateCommands(entity.Commands);

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

            // Key
            AddPrimaryKey(entity.Keys, sb);

            // Attributes
            AddAttributes(entity.Attributes, sb);

            // Relationships
            AddRelationships(entity.Relationships, sb);

            // Owned Relationships - define Aggregate Root boundaries
            AddRelationships(entity.OwnedRelationships, sb);

            sb.AppendLine(@"}");

            GenerateFile(sb, entity.Name);
        }

        private void AddPrimaryKey(IEnumerable<NoxSimpleTypeDefinition> keys, StringBuilder sb)
        {
            foreach (var key in keys)
            {
                if (key.Type == NoxType.Entity)
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
