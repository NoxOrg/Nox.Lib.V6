using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace Nox.Cli.Services;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_services.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object instance)
    {
        _services.AddSingleton(service, instance);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));

        _services.AddSingleton(service, (provider) => func());
    }
}