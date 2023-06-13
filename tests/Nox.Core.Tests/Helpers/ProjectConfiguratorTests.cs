using Moq;
using Nox.Core.Helpers;
using Nox.Core.Helpers.TextMacros;
using NUnit.Framework;

namespace Nox.Core.Tests.Helpers;

public class ProjectConfiguratorTests
{
    [Test]
    public void When_Read_ProjectDefinition_ShouldExpandEnvironmentVariables()
    {
        var environmentProvider = new Mock<IEnvironmentProvider>();

        environmentProvider.Setup(service => service.GetEnvironmentVariable("DB_USER")).Returns("sa");
        environmentProvider.Setup(service => service.GetEnvironmentVariable("DB_PASSWORD")).Returns("password1");

        var sut = new ProjectConfigurator("./", new ITextMacroParser[] { new TextEnvironmentVariableMacroParser(environmentProvider.Object) });
        var textExpanded  = sut.ReadDefinitionFile("./Helpers/ProjectConfiguratorTestsRaw.yaml");
        var textExpected  = sut.ReadDefinitionFile("./Helpers/ProjectConfiguratorTestsExpected.yaml");

        Assert.AreEqual(textExpected, textExpanded);
    }
}