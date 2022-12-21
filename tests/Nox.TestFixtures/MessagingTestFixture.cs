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
public class MessagingTestFixture: ConfigurationTestFixture
{
    [OneTimeSetUp]
    public void Setup()
    {
        TestServices.AddLogging();
        TestServices!
            .AddSingleton<IDynamicModel, DynamicModel>()
            .AddSingleton<IDynamicService, DynamicService>();
    }

    protected void BuildServiceProvider()
    {
        TestServiceProvider = TestServices!.BuildServiceProvider();
    }
}