
using Nox.Dynamic.Loaders;

namespace FO.Consumer;

public class INT001_Customer : INoxConsumer
{
    public IDictionary<string, object?> Value { get; set; } = null!;
    public NoxConsumerType Type { get; set; } 
}
