using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nox.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

var version = Assembly.GetEntryAssembly()?
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;

var services = new ServiceCollection();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.RunAsync(args);

