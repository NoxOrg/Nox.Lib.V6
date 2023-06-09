using Microsoft.CodeAnalysis;
using Nox.Generator.Generators;
using Nox.Solution;
using System.IO;
using System.Linq;

namespace Nox.Generator;

[Generator]
public class NoxDynamicGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        /*
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
        */
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var EntityGenerator = new EntityGenerator(context);

        context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"Executing Nox Generator"));

        var assemblyName = context.Compilation.AssemblyName;

        var mainSyntaxTree = context.Compilation.SyntaxTrees
            .FirstOrDefault(x => x.FilePath.EndsWith("Program.cs"));

        if (mainSyntaxTree == null)
        {
            return;
        }        

        var programPath = Path.GetDirectoryName(mainSyntaxTree.FilePath);
        var designRootFullPath = Path.GetFullPath(programPath!);

        // TODO: fix
        var noxConfig = new NoxSolutionBuilder()
            .UseYamlFile("./files/domain.solution.nox.yaml")
            .UseYamlFile("./files/application.solution.nox.yaml")
            .Build();

        var entities = noxConfig.Domain.Entities;

        if (!entities.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NW0001, null));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"{entities.Count} entities found"));

            foreach (var entity in entities)
            {
                context.ReportDiagnostic(Diagnostic.Create(WarningsErrors.NI0000, null, $"Adding Entity class: {entity.Name} from assembly {assemblyName}"));
                EntityGenerator.AddEntity(entity);
            }
        }

        var DtoGenerator = new DtoGenerator(context);
        foreach (var dto in noxConfig.Application.DataTransferObjects)
        {
            DtoGenerator.AddDTO(dto);
        }

        var dbContextGenerator = new DbContextGenerator(context);
        dbContextGenerator.AddDbContext(entities);

        // TODO: Generate controllers

        //if (EntityGenerator.AllQueries.Any() || EntityGenerator.AllCommands.Any())
        //{
        //    var webHelperGenerator = new WebHelperGenerator(context);
        //    webHelperGenerator.AddQueries(EntityGenerator.AllQueries, EntityGenerator.AllCommands);
        //}
    }
}