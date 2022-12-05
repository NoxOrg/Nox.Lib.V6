using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nox.Generator.TestFixture;
using NUnit.Framework;

namespace Nox.Generator.Tests;

public class GeneratorTests: GeneratorTestFixture
{
    [Test]
    public void Can_Generate_Dynamic_Entity_Sources()
    {
        var compilation = GenerateTestExeCompilation();
        Assert.That(compilation.SyntaxTrees.Count(), Is.EqualTo(1));
        var updatedComp = RunGenerators(compilation, out var generatorDiags, new NoxDynamicGenerator());
        Assert.That(updatedComp.SyntaxTrees.Count(), Is.EqualTo(9));
    }

    private Compilation RunGenerators(Compilation c, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
    {
        var driver = CSharpGeneratorDriver.Create(generators);
        driver.RunGeneratorsAndUpdateCompilation(c, out var updated, out diagnostics);
        return updated;
    }
    
}