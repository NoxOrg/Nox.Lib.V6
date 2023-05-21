using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityConfiguration : MetaBase
{
    public EntityKeyConfiguration Key { get; set; } = new();

    public List<EntityAttributeConfiguration> Attributes { get; set; } = new();

    public List<MessageTargetConfiguration>? Messaging { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PluralName { get; set; } = string.Empty;

    public string Schema { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public string Table { get; set; } = string.Empty;

    public bool IsAggregateRoot { get; set; } = false;

    public CrudEventsConfiguration RaiseCrudEvents { get; set; } = new();

    public List<EntityRelationshipConfiguration>? Relationships { get; set; } = new();

    public List<OwnedEntityRelationshipConfiguration>? OwnedRelationships { get; set; } = new();

    public List<CommandConfiguration>? Commands { get; set; } = new();

    public List<EventConfiguration>? Events { get; set; } = new();

    public List<QueryConfiguration>? Queries { get; set; } = new();
}