using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Nox.Generator.TestFixture;

[TestFixture]
public class GeneratorTestFixture
{
    public Compilation GenerateTestExeCompilation()
    {
        var programFile = File.ReadAllText("./TestExecutable/Program.cs");
        var syntaxTree = CSharpSyntaxTree.ParseText(programFile, CSharpParseOptions.Default, "./TestExecutable/Program.cs");
        var compilation = CSharpCompilation.Create("TestExe", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        return compilation;
    }
    
    
}