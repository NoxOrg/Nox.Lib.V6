namespace Nox;

public class ServiceValidationInfo
{
    public string ServiceName { get; private set; }
    public IReadOnlyDictionary<string, string> ConfigurationVariables { get; private set; }

    public ServiceValidationInfo(string serviceName, IReadOnlyDictionary<string, string> configurationVariables)
    {
        ServiceName = serviceName;

        ConfigurationVariables = configurationVariables;
    }
}