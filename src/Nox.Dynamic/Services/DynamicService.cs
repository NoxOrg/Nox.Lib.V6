using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.MetaData;
using System.Collections.ObjectModel;
using YamlDotNet.Serialization;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Nox.Dynamic.Exceptions;

namespace Nox.Dynamic.Services
{

    public class DynamicService
    {

        private ILogger _logger = null!;

        private Service _service = null!;
        public IReadOnlyDictionary<string, Entity> Entities => new ReadOnlyDictionary<string, Entity>(
            _service.Entities.ToDictionary(x => x.Name, x => x));

        public IReadOnlyDictionary<string, Api> Apis => new ReadOnlyDictionary<string, Api>(
            _service.Apis.ToDictionary(x => x.Name, x => x)
        );

        public ServiceDatabase ServiceDatabase => _service.Database;

        public string KeyVaultUri => _service.KeyVaultUri;

        public IReadOnlyCollection<Loader> Loaders => new ReadOnlyCollection<Loader>( _service.Loaders.ToList() );

        public async Task<bool> ExecuteDataLoadersAsync()
        {
            _logger.LogInformation("Executing data load tasks");

            if (_service.Database?.DatabaseProvider is not null)
            {   
                return await _service.Database.DatabaseProvider.LoadData(_service, _logger);
            }

            _logger.LogWarning("No database settings found in service definition");

            return false;
        }

        public class Builder
        {
            // Constants

            private const string SERVICE_DEFINITION_PATTERN = @"*.service.nox.yaml";

            private const string ENTITITY_DEFINITION_PATTERN = @"*.entity.nox.yaml";

            private const string LOADER_DEFINITION_PATTERN = @"*.loader.nox.yaml";

            private const string API_DEFINITION_PATTERN = @"*.api.nox.yaml";

            // Class def

            private readonly DynamicService _dynamicService;

            private readonly IDeserializer _deserializer;

            private ILogger _logger = null!;

            private IConfiguration _configuration = null!;

            public Builder()
            {
                _deserializer = new DeserializerBuilder().Build();

                _dynamicService = new DynamicService();
            }

            public Builder FromRootFolder(string rootFolder)
            {
                var service = ReadServiceDefinitionsFromFolder(rootFolder);

                service.Entities = ReadEntityDefinitionsFromFolder(rootFolder);

                service.Loaders = ReadLoaderDefinitionsFromFolder(rootFolder);

                service.Apis = ReadApiDefinitionsFromFolder(rootFolder);

                _dynamicService._service = service;

                return this;
            }

            public Builder WithLogger(ILogger logger)
            {
                _dynamicService._logger = logger;

                _logger = logger;

                return this;
            }

            public Builder WithConfiguration(IConfiguration configuration)
            {
                _configuration = configuration;

                return this;
            }

            public DynamicService Build()
            {

                _dynamicService._service.Validate( GetConfigurationVariables() );

                return _dynamicService;
            }

            private Service ReadServiceDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, SERVICE_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Take(1)
                    .Select(f => { 
                        var service = _deserializer.Deserialize<Service>(ReadDefinitionFile(f)); 
                        service.DefinitionFileName = Path.GetFullPath(f); 
                        service.Database.DefinitionFileName = Path.GetFullPath(f);
                        return service; 
                    })
                    .First();
            }

            private List<Entity> ReadEntityDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, ENTITITY_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => {
                        var entity = _deserializer.Deserialize<Entity>(ReadDefinitionFile(f));
                        entity.DefinitionFileName = Path.GetFullPath(f);
                        entity.Attributes.ToList().ForEach(a => {a.DefinitionFileName = Path.GetFullPath(f); });
                        return entity;
                    })
                    .ToList();
            }

            private List<Loader> ReadLoaderDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, LOADER_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => {
                        var loader = _deserializer.Deserialize<Loader>(ReadDefinitionFile(f));
                        loader.DefinitionFileName = Path.GetFullPath(f);
                        loader.Sources.ToList().ForEach(s => {s.DefinitionFileName = Path.GetFullPath(f); });
                        return loader;
                    })
                    .ToList();
            }

            private List<Api> ReadApiDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, API_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => {
                        var api = _deserializer.Deserialize<Api>(ReadDefinitionFile(f));
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

            private IReadOnlyDictionary<string,string> GetConfigurationVariables()
            {
                _logger.LogInformation("Resolving all configuration variables...");

                var databases = GetServiceDatabasesFromDefinition();

                // populate variables from application config

                var variables = databases
                    .Where(d => string.IsNullOrEmpty(d.ConnectionString))
                    .Where(d => !string.IsNullOrEmpty(d.ConnectionVariable))
                    .Select(d => d.ConnectionVariable ?? "")
                    .ToHashSet()
                    .ToDictionary(v => v, v => _configuration[v], StringComparer.OrdinalIgnoreCase);

                variables.Add("EtlBox:LicenseKey", _configuration["EtlBox:LicenseKey"]);

                // try key vault where app configuration is missing 
                if (variables.Any(v => v.Value == null))
                {
                    TryAddMissingConfigsFromKeyVault(_dynamicService._service.KeyVaultUri, variables);
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

                return variables;

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
                    _dynamicService._service.Database
                };

                foreach (var kvp in _dynamicService._service.Loaders)
                {
                    foreach (var db in kvp.Sources)
                    {
                        serviceDatabases.Add(db);
                    }
                }

                return serviceDatabases;
            }
        }
    }
}