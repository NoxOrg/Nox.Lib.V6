﻿using Microsoft.CodeAnalysis;
using Nox.Generator.Generators.Entities;
using System.Linq;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class DbContextGenerator : BaseGenerator
    {
        internal DbContextGenerator(GeneratorExecutionContext context)
            : base(context) { }

        internal bool AddDbContext(string[] dbSets, EntityWithCompositeKey[] entityWithCompositeKeys)
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"// autogenerated");
            sb.AppendLine(@"using Nox.Data;");
            sb.AppendLine(@"using Nox.Core.Interfaces.Database;");
            sb.AppendLine(@"using Microsoft.EntityFrameworkCore;");
            sb.AppendLine(@"using Microsoft.EntityFrameworkCore.Design;");
            sb.AppendLine(@"using MySql.EntityFrameworkCore.Extensions;");
            sb.AppendLine(@"using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine(@"");
            sb.AppendLine(@"namespace Nox;");
            sb.AppendLine(@"");

            // Dynamic DB context
            sb.AppendLine(@"/// <summary>");
            sb.AppendLine(@"/// Dynamic DB Context for OData endpoints");
            sb.AppendLine(@"/// </summary>");
            sb.AppendLine($@"public class NoxDbContext : DynamicDbContext");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    public NoxDbContext(");
            sb.AppendLine(@"        DbContextOptions<DynamicDbContext> options,");
            sb.AppendLine(@"        IDynamicModel dynamicDbModel");
            sb.AppendLine(@"    )");
            sb.AppendLine(@"    : base(options, dynamicDbModel) { }");
            sb.AppendLine(@"");
            sb.AppendLine(@"}");
            sb.AppendLine(@"");

            // Strongly typed DbContext for Queries / Commands
            sb.AppendLine(@"/// <summary>");
            sb.AppendLine(@"/// Strongly typed DbContext for Queries / Commands / Custom code");
            sb.AppendLine(@"/// </summary>");
            sb.AppendLine($@"public class NoxDomainDbContext : DbContext, IDynamicNoxDomainDbContext");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    // Dynamic model is needed to access the Data Provider");
            sb.AppendLine(@"    private readonly IDynamicModel _dynamicDbModel;");
            sb.AppendLine(@"");
            // Generate strongly typed DbSets for each aggreagate root or independent entity
            foreach (var setEntity in dbSets)
            {
                sb.AppendLine($@"    public DbSet<{setEntity}> {setEntity} {{ get; set; }}");
                sb.AppendLine(@"");
            }
            sb.AppendLine(@"    public NoxDomainDbContext(");
            sb.AppendLine(@"        DbContextOptions<NoxDomainDbContext> options,");
            sb.AppendLine(@"        IDynamicModel dynamicDbModel");
            sb.AppendLine(@"    )");
            sb.AppendLine(@"    : base(options)");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        _dynamicDbModel = dynamicDbModel;");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"");
            // Generate a method to register this DbContext implementation in the IoC container
            sb.AppendLine(@"    public static void RegisterContext(IServiceCollection services)");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        services.AddDbContext<NoxDomainDbContext>();");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"");
            sb.AppendLine(@"    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        var provider = _dynamicDbModel.GetDatabaseProvider();");
            sb.AppendLine(@"        provider.ConfigureDbContext(optionsBuilder);");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"");
            sb.AppendLine(@"    protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine(@"    {");
            // Set up composite keys
            foreach (var entity in entityWithCompositeKeys)
            {
                sb.AppendLine($@"        modelBuilder.Entity<{entity.EntityName}>().HasKey(new [] {{ ""{string.Join(@""", """, entity.KeyEntities.Select(k => $"{k}Id"))}"" }});");
            }
            sb.AppendLine(@"        base.OnModelCreating(modelBuilder);");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"}");
            sb.AppendLine(@"");

            // Add for MySql compatibility
            sb.AppendLine(@"// https://www.svrz.com/unable-to-resolve-service-for-type-microsoft-entityframeworkcore-storage-typemappingsourcedependencies/");
            sb.AppendLine(@"");
            sb.AppendLine(@"public class MysqlEntityFrameworkDesignTimeServices : IDesignTimeServices");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        serviceCollection.AddEntityFrameworkMySQL();");
            sb.AppendLine(@"        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)");
            sb.AppendLine(@"            .TryAddCoreServices();");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"}");

            GenerateFile(sb, "NoxDbContext");

            return true;
        }
    }
}