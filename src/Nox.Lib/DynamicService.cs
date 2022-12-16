using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Hangfire;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using Nox.Data;
using Nox.Entity.XtendedAttributes;
using Nox.Etl;
using Nox.Messaging;

namespace Nox.Lib;

public class DynamicService : IDynamicService
{
    private ILogger _logger;
    private readonly IMetaService _metaService;
    private readonly IEtlExecutor _etlExecutor;
    public string Name => _metaService.Name;
    public IMetaService MetaService => _metaService;
    public string KeyVaultUri => _metaService.KeyVaultUri;
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
        INoxConfiguration noxConfig,
        IEtlExecutor etlExecutor,
        IDataProviderFactory factory,
        INoxMessenger messenger)
    {
        _logger = logger;
        _etlExecutor = etlExecutor;

        _metaService = new Configurator(this)
            .WithLogger(_logger)
            .WithAppConfiguration(appConfig)
            .WithNoxConfiguration(noxConfig)
            .WithDatabaseProviderFactory(factory)
            .WithMessenger(messenger)
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
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Api.Api)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MessagingProvider)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(XtendedAttributeValue)), typeof(XtendedAttributeValue), "dbo");
    }
    
    public void SetupRecurringLoaderTasks()
    {
        var executor = _etlExecutor;

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

    public void EnsureDatabaseCreated(DbContext dbContext)
    {
        if (dbContext.Database.EnsureCreated())
        {
            dbContext.Add((MetaService)_metaService);
            dbContext.SaveChanges();    
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
                    if (prop.GetCustomAttributes(typeof(NotMappedAttribute), false).Length > 0)
                        continue;

                    var typeString = prop.PropertyType.Name.ToLower();

                    if (prop.Name == "Name" && typeString == "string")
                    {
                        b.Property(prop.Name).HasMaxLength(128);
                    }
                    else if (typeString == "decimal")
                    {
                        b.Property(prop.Name).HasPrecision(9, 6);
                    }
                    else if (typeString == "object")
                    {
                        var dbType = _metaService.Database.DataProvider!.ToDatabaseColumnType(new EntityAttribute() { Type = "object" });

                        b.Property(prop.Name).HasColumnType(dbType);
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
        private IMetaService? _service;
        private IConfiguration? _appConfig;
        private INoxConfiguration? _noxConfig;
#pragma warning disable IDE0052 // Remove unread private members
        private INoxMessenger? _messenger;
#pragma warning restore IDE0052 // Remove unread private members
        
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

        public Configurator WithAppConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
            return this;
        }

        public Configurator WithNoxConfiguration(INoxConfiguration noxConfig)
        {
            _noxConfig = noxConfig;
            return this;
        }
        
        public Configurator WithDatabaseProviderFactory(IDataProviderFactory factory)
        {
            _factory = factory;
            return this;
        }

        public Configurator WithMessenger(INoxMessenger messenger)
        {
            _messenger = messenger;
            return this;
        }
        
        public IMetaService Configure()
        {
            _service = _noxConfig!.ToMetaService();
            var serviceDatabases = GetServiceDatabasesFromNoxConfig();
            ResolveConfigurationVariables(serviceDatabases);

            _service.Validate();
            _service.Configure();
            // if (_messenger != null) _messenger.Configure(_service.MessagingProviders);
            
            serviceDatabases.ToList().ForEach(db =>
            {
                db.DataProvider = _factory!.Create(db.Provider);
                db.DataProvider.ConfigureServiceDatabase(db, _service.Name);
            });

            return _service;
        }
        
        private void ResolveConfigurationVariables(IList<IServiceDataSource> serviceDatabases)
        {
            _logger!.LogInformation("Resolving all configuration variables...");
            // populate variables from application config
            var databases = serviceDatabases
                .Where(d => string.IsNullOrEmpty(d.ConnectionString))
                .Where(d => !string.IsNullOrEmpty(d.ConnectionVariable));

            var variables = databases
                .Select(d => d.ConnectionVariable!)
                .ToHashSet()
                .ToDictionary(v => v, v => _appConfig?[v], StringComparer.OrdinalIgnoreCase);

            if (_service!.MessagingProviders is { Count: > 0 })
            {
                foreach (var item in _service.MessagingProviders)
                {
                    if (string.IsNullOrEmpty(item.ConnectionString) && !string.IsNullOrEmpty(item.ConnectionVariable))
                    {
                        variables.Add(item.ConnectionVariable, _appConfig?[item.ConnectionVariable]);
                    }
                }    
            }
            
            variables.Add("EtlBox:LicenseKey", _appConfig?["EtlBox:LicenseKey"]);

            // try key vault where app configuration is missing 
            if (variables.Any(v => v.Value == null))
            {
                TryAddMissingConfigsFromKeyVault(_service.KeyVaultUri, variables!);
            }

            if (variables.Any(v => v.Value == null))
            {
                var variableNames = string.Join(',', variables
                    .Where(v => v.Value == null)
                    .Select(v => v.Key)
                    .ToArray()
                );
                throw new ConfigurationNotFoundException(variableNames);
            }

            databases.ToList().ForEach(db =>
                db.ConnectionString = variables[db.ConnectionVariable!]
            );
            
            if (_service.MessagingProviders is { Count: > 0 })
            {
                foreach (var item in _service.MessagingProviders)
                {
                    if (string.IsNullOrEmpty(item.ConnectionString) && !string.IsNullOrEmpty(item.ConnectionVariable))
                    {
                        item.ConnectionString = variables[item.ConnectionVariable!];
                    }        
                }    
            }
        }

        private void TryAddMissingConfigsFromKeyVault(string vaultUri, Dictionary<string, string> variables)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            try
            {
                var test = keyVault.GetSecretAsync(vaultUri, "Test").GetAwaiter().GetResult().Value;
            }
            catch (Exception ex)
            {
                _logger!.LogWarning("...unable to connect to key vault because [{msg}]", ex.Message);
                return;
            }

            foreach (var key in variables.Keys)
            {

                if (string.IsNullOrEmpty(variables[key]))
                {
                    _logger!.LogInformation("...Resolving variable [{key}] from secrets vault {vault}", key, vaultUri);
                    variables[key] = keyVault.GetSecretAsync(vaultUri, key.Replace(":", "--")).GetAwaiter().GetResult().Value;
                }
                else
                {
                    _logger!.LogInformation("...Resolving variable [{key}] from app configuration", key);
                }

            }
        }

        private IList<IServiceDataSource> GetServiceDatabasesFromNoxConfig()
        {
            var serviceDatabases = new List<IServiceDataSource>
            {
                _service!.Database!
            };

            foreach (var dataSource in _service.DataSources!)
            {
                if (dataSource.Provider.Equals(DataProvider.JsonFile,StringComparison.OrdinalIgnoreCase)) continue;

                serviceDatabases.Add(dataSource);
            }

            return serviceDatabases;
        }

    }
}