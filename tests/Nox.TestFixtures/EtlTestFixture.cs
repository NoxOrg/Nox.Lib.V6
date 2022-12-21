using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class EtlTestFixture: ConfigurationTestFixture
{
    [OneTimeSetUp]
    public void Setup()
    {
        TestServices.AddLogging();
        TestServices!
            .AddDataProviderFactory()
            .AddDbContext<IDynamicDbContext, DynamicDbContext>()
            .AddSingleton<IDynamicModel, DynamicModel>()
            .AddSingleton<IDynamicService, DynamicService>();
        TestServices!.AddSingleton<SqlHelper>();
        TestServiceProvider = TestServices.BuildServiceProvider();
    }
}