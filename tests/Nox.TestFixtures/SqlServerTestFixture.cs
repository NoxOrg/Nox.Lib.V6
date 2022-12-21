using System;
using System.Threading;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class SqlServerTestFixture
{
    
    
    [OneTimeSetUp]
    public void SqlServerTestFixtureSetup()
    {
        
    }

    [OneTimeTearDown]
    public void SqlServerTestFixtureTeardown()
    {
        
    }

    
}