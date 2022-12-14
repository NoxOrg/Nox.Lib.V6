using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nox.Generator.TestFixture;
using NUnit.Framework;

namespace Nox.Generator.Tests;

public class GeneratorTests: GeneratorTestFixture
{
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources_From_AppSettings()
    {
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(5));
    }

    [Test]
    public void Must_handle_missing_design_folder()
    {
        var appSetFile = "./TestExecutable/appsettings.json";
        if (File.Exists($"{appSetFile}.tmp")) File.Delete($"{appSetFile}.tmp");
        File.Move(appSetFile, $"{appSetFile}.tmp");
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Empty");
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(1));
        File.Move($"{appSetFile}.tmp", appSetFile);
    }
    
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources_From_Env_AppSettings()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Test");
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(5));
    }
    
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources_From_Empty_AppSettings()
    {
        var appSetFile = "./TestExecutable/appsettings.json";
        if (File.Exists($"{appSetFile}.tmp")) File.Delete($"{appSetFile}.tmp");
        File.Move(appSetFile, $"{appSetFile}.tmp");
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Empty");
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(1));
        File.Move($"{appSetFile}.tmp", appSetFile);
    }

    private Compilation RunGenerators(Compilation c, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
    {
        var driver = CSharpGeneratorDriver.Create(generators);
        driver.RunGeneratorsAndUpdateCompilation(c, out var updated, out diagnostics);
        return updated;
    }
    
}