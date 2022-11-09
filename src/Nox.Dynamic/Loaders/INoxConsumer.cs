namespace Nox.Dynamic.Loaders
{
    public interface INoxConsumer
    {
        IDictionary<string, object?> Value { get; set; }
        public NoxConsumerType Type { get; set; }
    }

    public enum NoxConsumerType
    {
        Insert,
        Update
    }
}
