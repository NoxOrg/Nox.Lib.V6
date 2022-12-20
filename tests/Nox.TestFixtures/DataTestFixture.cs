using System;
using System.Collections.Immutable;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DataTestFixture: ConfigurationTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void Setup()
    {
        TestServices.AddLogging();
        TestServices.AddNox();
        TestServices.AddSingleton<TestSqlSeed>();
        TestServiceProvider = TestServices.BuildServiceProvider();
    }
}