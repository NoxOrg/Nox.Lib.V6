using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class ConfigurationTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    protected IServiceCollection? TestServices;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        TestServices = new ServiceCollection();
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        TestServices.AddSingleton<IConfiguration>(config);
        TestServiceProvider = TestServices.BuildServiceProvider();
    }
}