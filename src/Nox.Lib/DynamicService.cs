using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Core.Extensions;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Exceptions;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Models;
using Nox.Entity.XtendedAttributes;

namespace Nox.Lib;

public class DynamicService : IDynamicService
{
    private ILogger _logger;
    private readonly IProjectConfiguration _metaService;
    private readonly IEtlExecutor _etlExecutor;
    public string Name => _metaService.Name;
    public IProjectConfiguration MetaService => _metaService;
    public string KeyVaultUri => _metaService.KeyVaultUri;
    public bool AutoMigrations => _metaService.AutoMigrations;
    public IReadOnlyDictionary<string, IEntity>? Entities
    {
        get
        {
            if (_metaService.Entities != null)
                return new ReadOnlyDictionary<string, IEntity>(
                    _metaService.Entities.ToDictionary(x => x.Name, x => x));
            return null;
        }
    }
    public IReadOnlyDictionary<string, IApi>? Apis
    {
        get
        {
            if (_metaService.Apis != null)
                return new ReadOnlyDictionary<string, IApi>(
                    _metaService.Apis.ToDictionary(x => x.Name, x => x)
                );
            return null;
        }
    }
    public IEnumerable<ILoader>? Loaders
    {
        get
        {
            if (_metaService.Loaders != null) return new ReadOnlyCollection<ILoader>(_metaService.Loaders.ToList());
            return null;
        }
    }

    public DynamicService(ILogger<DynamicService> logger,
        IConfiguration appConfig,
        IEtlExecutor etlExecutor,
        IDataProviderFactory factory,
        IProjectConfiguration metaService)
    {
        _logger = logger;
        _etlExecutor = etlExecutor;

        _metaService = new Configurator(this)
            .WithLogger(_logger)
            .WithAppConfiguration(appConfig)
            .WithMetaService(metaService)
            .WithDatabaseProviderFactory(factory)
            .Configure();

    }

    public async Task<bool> ExecuteDataLoadersAsync()
    {
        _logger.LogInformation("Executing data load tasks");

        return await _etlExecutor.ExecuteAsync(_metaService);

    }

    public async Task<bool> ExecuteDataLoaderAsync(ILoader loader)
    {
        if (_metaService.Entities == null) return false;
        var entity = _metaService.Entities.First(e => e.Name.Equals(loader.Target!.Entity, StringComparison.OrdinalIgnoreCase));
        return await _etlExecutor.ExecuteLoaderAsync(_metaService, loader, entity);
    }

    public void AddMetadata(ModelBuilder modelBuilder)
    {
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MetaBase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DynamicService)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Loader)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DataSourceBase)), typeof(DataSourceBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(ServiceDatabase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Core.Models.Api)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MessagingProvider)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(XtendedAttributeValue)), typeof(XtendedAttributeValue), "dbo");
    }
    
    public void SetupRecurringLoaderTasks()
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

        foreach (var loader in Loaders!)
        {
            var loaderInstance = (Loader)loader;
            var entity = Entities![loaderInstance.Target!.Entity];
            //

            if (loaderInstance.Schedule!.RunOnStartup)
            {
                executor.ExecuteLoaderAsync(_metaService, loaderInstance, entity).GetAwaiter().GetResult();
            }

            RecurringJob.AddOrUpdate(
                $"{Name}.{loader.Name}",
                () => executor.ExecuteLoaderAsync(_metaService, loader, entity),
                loaderInstance.Schedule.CronExpression
            );
        }
    }

    public void EnsureDatabaseCreatedIfAutoMigrationsIsSet(DbContext dbContext)
    {
        if (_metaService.AutoMigrations)
        {
            if (dbContext.Database.EnsureCreated())
            {
                dbContext.Add((ProjectConfiguration)_metaService);
                dbContext.SaveChanges();
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
                _metaService.Database!.DataProvider!.ConfigureEntityTypeBuilder(b, metaType.Name, schema);

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
                        var dbType = _metaService.Database.DataProvider!.ToDatabaseColumnType(new EntityAttribute() { Type = "object" });
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
        private IProjectConfiguration _metaService = null!;
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

        public Configurator WithMetaService(IProjectConfiguration metaService)
        {
            _metaService = metaService;
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

        public IProjectConfiguration Configure()
        {
            var serviceDatabases = GetServiceDatabasesFromNoxConfig();
            
            serviceDatabases.ToList().ForEach(db =>
            {
                db.DataProvider = _factory!.Create(db.Provider);
                db.DataProvider.Configure(db, _metaService.Name);
            });

            return _metaService;
        }

        private IList<IServiceDataSource> GetServiceDatabasesFromNoxConfig()
        {
            var serviceDatabases = new List<IServiceDataSource>
            {
                _metaService!.Database!
            };

            foreach (var dataSource in _metaService.DataSources!)
            {
                serviceDatabases.Add(dataSource);
            }

            return serviceDatabases;
        }

    }
}