using Microsoft.CodeAnalysis;
using Nox.Generator.Generators;
using Nox.Solution;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Nox.Generator;

[Generator]
public class NoxDynamicGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var EntityGenerator = new EntityGenerator(context);

        context.ReportDiagnosticsDebug("Executing Nox Generator");

        var assemblyName = context.Compilation.AssemblyName;

        var mainSyntaxTree = context.Compilation.SyntaxTrees
            .FirstOrDefault(x => x.FilePath.EndsWith("Program.cs"));

        var programPath = Path.GetDirectoryName(mainSyntaxTree.FilePath);
        var designRootFullPath = Path.GetFullPath(programPath!);

        if (mainSyntaxTree == null)
        {
            return;
        }

        NoxSolution noxConfig;
        try
        {
            // Read and check Nox configuration
            noxConfig = new NoxSolutionBuilder()
                .Build();
        }
        catch (Exception ex)
        {
            context.ReportDiagnosticsError(ex.Message);
            return;
        }

        if (noxConfig == null || noxConfig.Domain == null)
        {
            context.ReportDiagnosticsError("Unable to load Nox configuration and domain.");
            return;
        }

        if (noxConfig.Application == null)
        {
            context.ReportDiagnosticsError("Unable to load Nox Application configuration.");
            return;
        }

        // Generate Domain
        var entities = noxConfig.Domain.Entities;

        if (!entities.Any())
        {
            context.ReportDiagnosticsError("No entities found.");
        }
        else
        {
            context.ReportDiagnosticsDebug($"{entities.Count} entities found");
            
            var apiGenerator = new ControllerGenerator(context);

            foreach (var entity in entities)
            {
                context.ReportDiagnosticsDebug($"Adding Entity class: {entity.Name} from assembly {assemblyName}");
                EntityGenerator.AddEntity(entity);

                // Generate API controllers
                if (entity.Queries != null && entity.Queries.Any() || entity.Commands != null && entity.Commands.Any())
                {                    
                    apiGenerator.GenerateController(entity);
                }
            }
        }

        // Generate DTO
        if (noxConfig.Application.DataTransferObjects != null)
        {
            var DtoGenerator = new DtoGenerator(context);
            foreach (var dto in noxConfig.Application.DataTransferObjects)
            {
                DtoGenerator.AddDTO(dto);
            }
        }

        var dbContextGenerator = new DbContextGenerator(context);
        dbContextGenerator.AddDbContext(entities);
    }
}