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
using Nox.Core.Models;
using Nox.Data;
using Nox.Entity.XtendedAttributes;
using Nox.Etl;
using Nox.Microservice.Extensions;
using YamlDotNet.Serialization;

namespace Nox.Microservice;

public class DynamicService : IDynamicService
{
    private ILogger _logger;

    private readonly IMetaService _service;

    private readonly IEtlExecutor _etlExecutor;

    public string Name => _service.Name;

    public IMetaService MetaService => _service;

    public string KeyVaultUri => _service.KeyVaultUri;

    public IReadOnlyDictionary<string, IEntity>? Entities
    {
        get
        {
            if (_service.Entities != null)
                return new ReadOnlyDictionary<string, IEntity>(
                    _service.Entities.ToDictionary(x => x.Name, x => x));
            return null;
        }
    }

    public IReadOnlyDictionary<string, IApi>? Apis
    {
        get
        {
            if (_service.Apis != null)
                return new ReadOnlyDictionary<string, IApi>(
                    _service.Apis.ToDictionary(x => x.Name, x => x)
                );
            return null;
        }
    }

    public IEnumerable<ILoader>? Loaders
    {
        get
        {
            if (_service.Loaders != null) return new ReadOnlyCollection<ILoader>(_service.Loaders.ToList());
            return null;
        }
    }


    public DynamicService(ILogger<DynamicService> logger,
        IConfiguration configuration,
        IEtlExecutor etlExecutor,
        IDatabaseProviderFactory factory)
    {
        _logger = logger;

        var configuration1 = configuration;

        _etlExecutor = etlExecutor;

        _service = new Configurator(this)
            .WithLogger(_logger)
            .WithConfiguration(configuration1)
            .WithDatabaseProviderFactory(factory)
            .FromRootFolder(configuration1["Nox:DefinitionRootPath"])
            .Configure();

    }

    public async Task<bool> ExecuteDataLoadersAsync()
    {
        _logger.LogInformation("Executing data load tasks");

        return await _etlExecutor.ExecuteAsync(_service);

    }

    public async Task<bool> ExecuteDataLoaderAsync(ILoader loader, IDatabaseProvider destinationDbProvider)
    {
        if (_service.Entities == null) return false;
        var entity = _service.Entities.First(e => e.Name.Equals(loader.Target!.Entity, StringComparison.OrdinalIgnoreCase));
        return await _etlExecutor.ExecuteLoaderAsync(loader, destinationDbProvider, entity);
    }

    public void AddMetadata(ModelBuilder modelBuilder)
    {
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(MetaBase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DynamicService)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Loader)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(DatabaseBase)), typeof(DatabaseBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(ServiceDatabase)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
        AddMetadataFromNamespace(modelBuilder, Assembly.GetAssembly(typeof(Api.Api)), typeof(MetaBase), DatabaseObject.MetadataSchemaName);
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
                executor.ExecuteLoaderAsync(loaderInstance, _service.Database!.DatabaseProvider, entity).GetAwaiter().GetResult();
            }

            RecurringJob.AddOrUpdate(
                $"{Name}.{loader.Name}",
                () => executor.ExecuteLoaderAsync(loader, _service.Database!.DatabaseProvider!, entity),
                loaderInstance.Schedule.CronExpression
            );
        }

    }

    public void EnsureDatabaseCreated(DbContext dbContext)
    {
        if (dbContext.Database.EnsureCreated())
        {
            dbContext.Add((MetaService)_service);
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
                _service.Database!.DatabaseProvider.ConfigureEntityTypeBuilder(b, metaType.Name, schema);

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
                        var dbType = _service.Database.DatabaseProvider!.ToDatabaseColumnType(new EntityAttribute() { Type = "object" });

                        b.Property(prop.Name).HasColumnType(dbType);
                    }
                }
            });
        }
    }

    private class Configurator
    {
        // Class def

        private readonly DynamicService _dynamicService;

        private readonly IDeserializer _deserializer;

        private ILogger _logger = null!;

        private IConfiguration _configuration = null!;

        private IDatabaseProviderFactory _factory = null!;

        private MetaService _service = null!;

        public Configurator(DynamicService dynamicService)
        {
            _deserializer = new DeserializerBuilder()
                .Build();

            _dynamicService = dynamicService;
        }

        public Configurator FromRootFolder(string rootFolder)
        {
            var service = ReadServiceDefinitionsFromFolder(rootFolder);

            service.Entities = ReadEntityDefinitionsFromFolder(rootFolder);

            service.Loaders = ReadLoaderDefinitionsFromFolder(rootFolder);

            service.Apis = ReadApiDefinitionsFromFolder(rootFolder);

            _service = service;

            return this;
        }

        public Configurator WithLogger(ILogger logger)
        {
            _dynamicService._logger = logger;

            _logger = logger;

            return this;
        }

        public Configurator WithConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;

            return this;
        }

        public Configurator WithDatabaseProviderFactory(IDatabaseProviderFactory factory)
        {
            _factory = factory;

            return this;
        }

        public MetaService Configure()
        {
            var serviceDatabases = GetServiceDatabasesFromDefinition();
            ResolveConfigurationVariables(serviceDatabases);

            _service.Validate();
            _service.Configure();
            
            serviceDatabases.ToList().ForEach(db =>
            {
                db.DatabaseProvider = _factory.Create(db.Provider);
                db.DatabaseProvider.ConfigureServiceDatabase(db, _service.Name);
            });

            return _service;
        }

        private MetaService ReadServiceDefinitionsFromFolder(string rootFolder)
        {
            return Directory
                .EnumerateFiles(rootFolder, FileExtension.ServiceDefinition, SearchOption.AllDirectories)
                .Take(1)
                .Select(f =>
                {
                    var service = _deserializer.Deserialize<MetaService>(ReadDefinitionFile(f));
                    service.DefinitionFileName = Path.GetFullPath(f);
                    service.Database!.DefinitionFileName = Path.GetFullPath(f);
                    return service;
                })
                .First();
        }

        private List<Core.Models.Entity> ReadEntityDefinitionsFromFolder(string rootFolder)
        {
            return Directory
                .EnumerateFiles(rootFolder, FileExtension.EntityDefinition, SearchOption.AllDirectories)
                .Select(f =>
                {
                    var entity = _deserializer.Deserialize<Core.Models.Entity>(ReadDefinitionFile(f));
                    entity.DefinitionFileName = Path.GetFullPath(f);
                    entity.Attributes.ToList().ForEach(a => { a.DefinitionFileName = Path.GetFullPath(f); });
                    return entity;
                })
                .ToList();
        }

        private List<Loader> ReadLoaderDefinitionsFromFolder(string rootFolder)
        {
            var loaders = Directory
                .EnumerateFiles(rootFolder, FileExtension.LoaderDefinition, SearchOption.AllDirectories)
                .Select(f =>
                {
                    var loader = _deserializer.Deserialize<Loader>(ReadDefinitionFile(f)) as Loader;
                    loader!.DefinitionFileName = Path.GetFullPath(f);
                    loader.Sources!.ToList().ForEach(s => { s.DefinitionFileName = Path.GetFullPath(f); });
                    return loader;
                });
            return loaders.ToList();
        }

        private List<Api.Api> ReadApiDefinitionsFromFolder(string rootFolder)
        {
            return Directory
                .EnumerateFiles(rootFolder, FileExtension.ApiDefinition, SearchOption.AllDirectories)
                .Select(f =>
                {
                    var api = _deserializer.Deserialize<Api.Api>(ReadDefinitionFile(f));
                    api.DefinitionFileName = Path.GetFullPath(f);
                    return api;
                })
                .ToList();
        }


        private string ReadDefinitionFile(string fileName)
        {
            _logger.LogInformation("Reading definition from {fileName}", fileName.Replace('\\', '/'));

            return File.ReadAllText(fileName);
        }

        private void ResolveConfigurationVariables(IList<IServiceDatabase> serviceDatabases)
        {
            _logger.LogInformation("Resolving all configuration variables...");

            // populate variables from application config

            var databases = serviceDatabases
                .Where(d => string.IsNullOrEmpty(d.ConnectionString))
                .Where(d => !string.IsNullOrEmpty(d.ConnectionVariable));

            var variables = databases
                .Select(d => d.ConnectionVariable!)
                .ToHashSet()
                .ToDictionary(v => v, v => _configuration[v], StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(_service.MessageBus!.ConnectionString) &&
                !string.IsNullOrEmpty(_service.MessageBus.ConnectionVariable))
            {
                variables.Add(_service.MessageBus.ConnectionVariable,
                    _configuration[_service.MessageBus.ConnectionVariable]);
            }

            variables.Add("EtlBox:LicenseKey", _configuration["EtlBox:LicenseKey"]);

            // try key vault where app configuration is missing 
            if (variables.Any(v => v.Value == null))
            {
                TryAddMissingConfigsFromKeyVault(_service.KeyVaultUri, variables);
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

            if (string.IsNullOrEmpty(_service.MessageBus.ConnectionString) &&
                !string.IsNullOrEmpty(_service.MessageBus.ConnectionVariable))
            {
                _service.MessageBus.ConnectionString = variables[_service.MessageBus.ConnectionVariable!];
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
                _logger.LogWarning("...unable to connect to key vault because [{msg}]", ex.Message);
                return;
            }

            foreach (var key in variables.Keys)
            {

                if (variables[key] == null)
                {
                    _logger.LogInformation("...Resolving variable [{key}] from secrets vault {vault}", key, vaultUri);
                    variables[key] = keyVault.GetSecretAsync(vaultUri, key.Replace(":", "--")).GetAwaiter().GetResult().Value;
                    _logger.LogInformation($"Variable [{key}] resolved to: {variables[key]}");
                }
                else
                {
                    _logger.LogInformation("...Resolving variable [{key}] from app configuration", key);
                }

            }
        }

        private IList<IServiceDatabase> GetServiceDatabasesFromDefinition()
        {
            var serviceDatabases = new List<IServiceDatabase>
            {
                _service.Database!
            };

            foreach (var loader in _service.Loaders!)
            {
                var kvp = (Loader)loader;
                foreach (var db in kvp.Sources!)
                {
                    serviceDatabases.Add(db);
                }
            }

            return serviceDatabases;
        }
    }
}