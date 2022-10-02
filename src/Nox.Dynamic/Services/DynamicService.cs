using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Dto;
using Nox.Dynamic.Loaders.Providers;
using Nox.Dynamic.Migrations.Providers;
using System.Collections.ObjectModel;
using YamlDotNet.Serialization;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using System.Data.SqlClient;
using Nox.Dynamic.Exceptions;

namespace Nox.Dynamic.Services
{
    public class DynamicService
    {

        private IConfiguration _configuration = null!;

        private ILogger _logger = null!;

        private Service _service = null!;
        public IReadOnlyDictionary<string, Entity> Entities => new ReadOnlyDictionary<string, Entity>(_service.Entities);

        public IReadOnlyDictionary<string, Api> Apis => new ReadOnlyDictionary<string, Api>(
            _service.Apis.Values.ToDictionary(x => x.Name, x => x)
        );

        public string? DatabaseConnectionString() => _service.Database.ConnectionString;


        public async Task<bool> ValidateDatabaseSchemaAsync()
        {
            _logger.LogInformation("Validating the database schema");

            if (_service.Database is not null)
            {
                var migrationProvider = new SqlServerMigrationProvider(_logger);

                return await migrationProvider.ValidateDatabaseSchemaAsync(_service);
            }

            _logger.LogWarning("No database settings found in service definition");

            return false;
        }

        public async Task<bool> ExecuteDataLoadersAsync()
        {
            _logger.LogInformation("Executing data load tasks");

            if (_service.Database is not null)
            {
                var loaderProvider = new SqlServerLoaderProvider(_logger);

                return await loaderProvider.ExecuteLoadersAsync(_service);
            }

            _logger.LogWarning("No database settings found in service definition");

            return false;
        }

        public class Builder
        {
            // Constants

            private const string SERVICE_DEFINITION_PATTERN = @"*.service.yaml";

            private const string ENTITITY_DEFINITION_PATTERN = @"*.entity.yaml";

            private const string LOADER_DEFINITION_PATTERN = @"*.loader.yaml";
            
            private const string API_DEFINITION_PATTERN = @"*.api.yaml";

            // Class def

            private readonly DynamicService _dynamicService;

            private readonly IDeserializer _deserializer;

            private ILogger _logger = null!;

            public Builder()
            {
                _deserializer = new DeserializerBuilder().Build();

                _dynamicService = new DynamicService();
            }

            public Builder FromRootFolder(string rootFolder)
            {
                var service = ReadServiceDefinitionsFromFolder(rootFolder);

                service.Entities = ReadEntityDefinitionsFromFolder(rootFolder)
                    .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

                service.Loaders = ReadLoaderDefinitionsFromFolder(rootFolder)
                    .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

                service.Apis = ReadApiDefinitionsFromFolder(rootFolder)
                    .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

                _dynamicService._service = service;

                return this;
            }

            public Builder WithConfiguration(IConfiguration configuration)
            {
                _dynamicService._configuration = configuration;

                return this;
            }

            public Builder WithLogger(ILogger logger)
            {
                _dynamicService._logger = logger;

                _logger = logger;

                return this;
            }

            public DynamicService Build()
            {
                ResolveAllConnectionStrings();

                return _dynamicService;
            }


            private Service ReadServiceDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, SERVICE_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Take(1)
                    .Select(f => _deserializer.Deserialize<Service>(ReadDefinitionFile(f)))
                    .First();
            }

            private List<Entity> ReadEntityDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, ENTITITY_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<Entity>(ReadDefinitionFile(f)))
                    .ToList();
            }

            private List<Loader> ReadLoaderDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, LOADER_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<Loader>(ReadDefinitionFile(f)))
                    .ToList();
            }

            private List<Api> ReadApiDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, API_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<Api>(ReadDefinitionFile(f)))
                    .ToList();
            }

            private string ReadDefinitionFile(string fileName)
            {
                _logger.LogInformation("Reading definition from {fileName}", fileName.Replace('\\', '/'));

                return File.ReadAllText(fileName);
            }

            private void ResolveAllConnectionStrings()
            {
                _logger.LogInformation("Resolving all connection strings...");

                var config = _dynamicService._configuration;

                var vaultUri = _dynamicService._service.KeyVaultUri;

                var databases = GetServiceDatabasesFromDefinition();

                // populate variables from application config

                var variables = databases
                    .Where(d => string.IsNullOrEmpty(d.ConnectionString))
                    .Where(d => !string.IsNullOrEmpty(d.ConnectionVariable))
                    .Select(d => d.ConnectionVariable ?? "")
                    .ToHashSet()
                    .ToDictionary(v => v, v => config[v], StringComparer.OrdinalIgnoreCase);

                // try key vault where app configuration is missing 
                if (variables.Any(v => v.Value == null))
                {
                    TryAddMissingConfigsFromKeyVault(vaultUri, variables);
                }

                if (variables.Any(v => v.Value == null))
                {
                    var variableNames = string.Join( ',', variables
                        .Where(v => v.Value == null)
                        .Select(v => v.Key)
                        .ToArray()
                    );
                    throw new ConfigurationNotFoundException(variableNames);
                }

                foreach (var db in databases)
                {
                    ResolveConnectionString(db, variables);
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
                        _logger.LogInformation("...Resolving ConnectionVariable [{key}] from secrets vault {vault}", key, vaultUri);
                        variables[key] = keyVault.GetSecretAsync(vaultUri, key.Replace(":", "--")).GetAwaiter().GetResult().Value;
                    }
                    else
                    {
                        _logger.LogInformation("...Resolving ConnectionVariable [{key}] from app configuration", key);
                    }

                }
            }

            private IList<ServiceDatabase> GetServiceDatabasesFromDefinition()
            {
                var serviceDatabases = new List<ServiceDatabase>
                {
                    _dynamicService._service.Database
                };

                foreach (var kvp in _dynamicService._service.Loaders)
                {
                    foreach (var db in kvp.Value.Sources)
                    {
                        serviceDatabases.Add(db);
                    }
                }

                return serviceDatabases;
            }

            private static void ResolveConnectionString(ServiceDatabase serviceDb, IDictionary<string,string> variables)
            {
                if (!string.IsNullOrEmpty(serviceDb.ConnectionString))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(serviceDb.ConnectionVariable))
                {
                    var connectionString = variables[serviceDb.ConnectionVariable];

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        serviceDb.ConnectionString = connectionString;
                        return;
                    }

                }

                var csb = new SqlConnectionStringBuilder(serviceDb.Options)
                {
                    DataSource = serviceDb.Server,
                    UserID = serviceDb.User,
                    Password = serviceDb.Password,
                    InitialCatalog = serviceDb.Name
                };

                serviceDb.ConnectionString = csb.ConnectionString;

                return;

            }


        }


    }
}