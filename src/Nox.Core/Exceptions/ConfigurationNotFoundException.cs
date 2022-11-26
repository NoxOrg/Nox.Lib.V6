
namespace Nox.Core.Exceptions
{
    public class ConfigurationNotFoundException : Exception
    {
        public ConfigurationNotFoundException(string variableName) 
            : base($"Variable(s) \"{variableName}\" was not found in configuration or vault.")
        {
        }
    }

}
