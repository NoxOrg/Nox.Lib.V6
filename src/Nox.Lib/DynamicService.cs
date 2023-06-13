using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Etl;
using Nox.Entity.XtendedAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Humanizer;
using Nox.Core.Models.Entity;
using Nox.Data.Extensions;
using Nox.Solution;

namespace Nox.Lib;

public class DynamicService : IDynamicService
{
    private ILogger _logger;
    private readonly NoxSolution _solution;
    private readonly IEtlExecutor _etlExecutor;
    private IServiceDataSource _entityStore;
    
    public string Name => _solution.Name;
    public NoxSolution Solution => _solution;

    public IServiceDataSource EntityStore => _entityStore;
    
    //Todo change this to new structure
    //public string KeyVaultUri => _metaService.KeyVaultUri;
    
    public IReadOnlyDictionary<string, Solution.Entity>? Entities
    {
        get
        {
            if (_solution.Domain is { Entities: not null })
                return new ReadOnlyDictionary<string, Solution.Entity>(
                    _solution.Domain.Entities.ToDictionary(x => x.Name, x => x));
            return null;
        }
    }
    // public IReadOnlyDictionary<string, IApi>? Apis
    // {
    //     get
    //     {
    //         if (_solution != null)
    //             return new ReadOnlyDictionary<string, IApi>(
    //                 _metaService.Apis.ToDictionary(x => x.Name, x => x)
    //             );
    //         return null;
    //     }
    // }
    
    public IEnumerable<Integration>? Integrations
    {
        get
        {
            if (_solution.Application is { Integrations.Count: > 0 })
            {
                return new ReadOnlyCollection<Integration>(_solution.Application.Integrations.ToList());
            }
            return null;
        }
    }

    public DynamicService(ILogger<DynamicService> logger,
        IConfiguration appConfig,
        IEtlExecutor etlExecutor,
        IDataProviderFactory factory,
        NoxSolution solution)
    {
        _logger = logger;
        _etlExecutor = etlExecutor;

        _solution = new Configurator(this)
            .WithLogger(_logger)
            .WithAppConfiguration(appConfig)
            .WithSolution(solution)
            .WithDatabaseProviderFactory(factory)
            .Configure();
    }

    public async Task<bool> ExecuteIntegrationsAsync()
    {
        _logger.LogInformation("Executing data load tasks");
        return await _etlExecutor.ExecuteAsync(_solution);
    }

    public async Task<bool> ExecuteIntegrationAsync(Integration integration)
    {
        if (_solution.Domain?.Entities == null) return false;
        //todo fix this
        //var entity = _solution.Domain.Entities.First(e => e.Name.Equals(integration.Target!.Entity, StringComparison.OrdinalIgnoreCase));
        var entity = new Solution.Entity();
        return await _etlExecutor.ExecuteEtlAsync(_solution, integration, entity);
    }

    public void AddMetadata(ModelBuilder modelBuilder)
    {
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MetaBase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DynamicService)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Solution.Solution)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DataSourceBase)), typeof(DataSourceBase), DatabaseObject.MetadataSchemaName);
        // AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(ServiceDatabase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        // AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Core.Models.Api)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        // AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MessagingProvider)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(XtendedAttributeValue)), typeof(XtendedAttributeValue), "dbo");
    }
    
    public void SetupRecurringIntegrationTasks()
    {
        if (Integrations != null && Integrations.Any())
        {
            var executor = _etlExecutor;
        
            //Remove old jobs
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            // setup recurring jobs based on cron schedule

            foreach (var integration in Integrations)
            {
                // todo loaders no longer exist, need to change this process
                // if (integration.Target?.TargetType == EtlTargetType.Entity)
                // {
                //     var entity = Entities![integration.Target.Name];
                // }
                //
                // if (integration.Source?.Schedule?.RunOnStartup == true)
                //     executor.ExecuteEtlAsync(_solution, integration, )
                //     if (loaderInstance.Schedule!.RunOnStartup)
                //     {
                //         executor.ExecuteEtlAsync(_metaService, loaderInstance, entity).GetAwaiter().GetResult();
                //     }
                //
                // RecurringJob.AddOrUpdate(
                //     $"{Name}.{loader.Name}",
                //     () => executor.ExecuteEtlAsync(_metaService, loader, entity),
                //     loaderInstance.Schedule.CronExpression
                // );
            }
        } 
    }

    private void AddMetadataFromNamespace(ModelBuilder modelBuilder, Assembly? assembly, Type baseType, string schema)
    {
        if (assembly == null) return;
        var nsTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(baseType))
            .Where(t => t.IsClass && t.IsSealed && t.IsPublic);

        foreach (var metaType in nsTypes)
        {
            modelBuilder.Entity(metaType, b =>
            {
                _entityStore!.DataProvider!.ConfigureEntityTypeBuilder(b, metaType.Name, schema);

                var entityTypes = modelBuilder.Model.GetEntityTypes();

                var properties = entityTypes
                    .First(e => e.ClrType.Name == metaType.Name)
                    .ClrType
                    .GetProperties();


                foreach (var prop in properties)
                {
                    if (prop.GetCustomAttributes<NotMappedAttribute>().Any())
                        continue;

                    var typeString = prop.PropertyType.Name.ToLower();

                    if (prop.Name == "Name" && typeString == "string")
                    {
                        b.Property(prop.Name).HasMaxLength(128);
                        continue;
                    }
                    if (typeString == "string")
                    {
                        var attr = prop.GetCustomAttributes<MaxLengthAttribute>(false);
                        if (attr.Any())
                        {
                            b.Property(prop.Name).HasMaxLength(attr.First().Length);
                        }
                        else
                        {
                            b.Property(prop.Name).HasMaxLength(128);
                        }
                    }
                    else if (typeString == "decimal")
                    {
                        b.Property(prop.Name).HasPrecision(38, 18);
                    }
                    else if (typeString == "object")
                    {
                        //todo set this to object type
                        var dbType = _entityStore.DataProvider!.ToDatabaseColumnType(new NoxSimpleTypeDefinition());
                        if (dbType == null)
                        {
                            b.Ignore(prop.Name);
                        }
                        else
                        {
                            b.Property(prop.Name).HasColumnType(dbType);
                        }
                    }
                }
            });
        }
    }

    private class Configurator
    {
        private readonly DynamicService _dynamicService;
        private IDataProviderFactory? _factory;
        private ILogger? _logger;
        private NoxSolution _solution = null!;
        private IConfiguration? _appConfig;

        public Configurator(DynamicService dynamicService)
        {
            _dynamicService = dynamicService;
        }

        public Configurator WithLogger(ILogger logger)
        {
            _dynamicService._logger = logger;
            _logger = logger;
            return this;
        }

        public Configurator WithSolution(NoxSolution solution)
        {
            _solution = solution;
            return this;
        }

        public Configurator WithAppConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
            return this;
        }

        public Configurator WithDatabaseProviderFactory(IDataProviderFactory factory)
        {
            _factory = factory;
            return this;
        }

        public NoxSolution Configure()
        {
            var serviceDatabases = GetServiceDatabasesFromNoxSolution();
            serviceDatabases.ToList().ForEach(db =>
            {
                db.DataProvider = _factory!.Create(db.Provider);
                db.DataProvider.Configure(db, _solution.Name);
            });

            return _solution;
        }

        private IList<IServiceDataSource> GetServiceDatabasesFromNoxSolution()
        {
            var serviceDatabases = new List<IServiceDataSource>();
            if (_solution.Infrastructure != null)
            {
                //Nox Entity Store
                serviceDatabases.Add(_solution.Infrastructure.Persistence.DatabaseServer.ToEntityStore());
                
                if (_solution.Infrastructure.Dependencies is { DataConnections: not null } && _solution.Infrastructure.Dependencies.DataConnections.Any())
                {
                    foreach (var dataConnection in _solution.Infrastructure.Dependencies.DataConnections)
                    {
                        serviceDatabases.Add(dataConnection.ToDataSource());
                    }
                }
            }
            return serviceDatabases;
        }

    }
}