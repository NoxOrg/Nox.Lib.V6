using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class DataConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = "localhost";
    public string User { get; set; } = "user";
    public int Port { get; set; } = 0;
    public string Password { get; set; } = "password";
    public string Options { get; set; } = "";
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
    public string Path { get; set; } = "";

}