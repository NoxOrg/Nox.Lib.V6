using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class CrudEventsConfiguration : MetaBase
{
    public bool Create { get; set; } = true;

    public bool Update { get; set; } = true;

    public bool Delete { get; set; } = false;
}