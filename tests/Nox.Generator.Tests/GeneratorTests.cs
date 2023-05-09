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
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(6));
    }

    [Test]
    public void Must_handle_missing_design_folder()
    {
        var appSetFile = "./TestExecutable/appsettings.json";
        if (File.Exists($"{appSetFile}.tmp")) File.Delete($"{appSetFile}.tmp");
        File.Move(appSetFile, $"{appSetFile}.tmp");
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(2));
        File.Move($"{appSetFile}.tmp", appSetFile);
    }
    
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources_From_Empty_AppSettings()
    {
        var appSetFile = "./TestExecutable/appsettings.json";
        if (File.Exists($"{appSetFile}.tmp")) File.Delete($"{appSetFile}.tmp");
        File.Move(appSetFile, $"{appSetFile}.tmp");
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(2));
        File.Move($"{appSetFile}.tmp", appSetFile);
    }
    
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources_From_Bloated_AppSettings()
    {
        var appSetFile = "./TestExecutable/appsettings.json";
        var bloatFile = "./TestExecutable/appsettings.bloated.json";
        if (File.Exists($"{appSetFile}.tmp")) File.Delete($"{appSetFile}.tmp");
        File.Move(appSetFile, $"{appSetFile}.tmp");
        File.Copy(bloatFile, appSetFile, true);
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(6));
        File.Move($"{appSetFile}.tmp", appSetFile, true);
    }

    private Compilation RunGenerators(Compilation c, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
    {
        var driver = CSharpGeneratorDriver.Create(generators);
        driver.RunGeneratorsAndUpdateCompilation(c, out var updated, out diagnostics);
        return updated;
    }
    
}