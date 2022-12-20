using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class ConfigurationTestFixture
{
    protected IServiceCollection TestServices = new ServiceCollection();
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        TestServices.AddSingleton<IConfiguration>(config);
    }
    
}