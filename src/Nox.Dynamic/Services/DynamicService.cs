﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.MetaData;
using System.Collections.ObjectModel;
using YamlDotNet.Serialization;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Nox.Dynamic.Exceptions;
using Nox.Dynamic.Loaders;
using Nox.Data;

namespace Nox.Dynamic.Services
{

    public class DynamicService : IDynamicService
    {

        private ILogger _logger = null!;

        private readonly Service _service = null!;

        private readonly IConfiguration _configuration = null!;

        private readonly ILoaderExecutor _loaderExector;

        public string Name => _service.Name;

        public Service Service => _service;

        public IServiceDatabase ServiceDatabase => _service.Database;

        public string KeyVaultUri => _service.KeyVaultUri;

        public IReadOnlyDictionary<string, Entity> Entities => new ReadOnlyDictionary<string, Entity>(
            _service.Entities.ToDictionary(x => x.Name, x => x));

        public IReadOnlyDictionary<string, Api> Apis => new ReadOnlyDictionary<string, Api>(
            _service.Apis.ToDictionary(x => x.Name, x => x)
        );

        public IReadOnlyCollection<Loader> Loaders => new ReadOnlyCollection<Loader>(_service.Loaders.ToList());


        public DynamicService(ILogger<DynamicService> logger, IConfiguration configuration, ILoaderExecutor loaderExector)
        {
            _logger = logger;

            _configuration = configuration;

            _loaderExector = loaderExector;

            _service = new Configurator(this)
                .WithLogger(_logger)
                .WithConfiguration(_configuration)
                .FromRootFolder(_configuration["Nox:DefinitionRootPath"])
                .Configure();

        }

        public async Task<bool> ExecuteDataLoadersAsync()
        {
            _logger.LogInformation("Executing data load tasks");

            return await _loaderExector.ExecuteAsync(_service);

        }

        public async Task<bool> ExecuteDataLoaderAsync(Loader loader, IDatabaseProvider destinationDbProvider)
        {
            var entity = _service.Entities.First(e => e.Name.Equals(loader.Target.Entity, StringComparison.OrdinalIgnoreCase));

            return await _loaderExector.ExecuteLoaderAsync(loader, destinationDbProvider, entity);
        }

        private class Configurator
        {
            // Constants



            // Class def

            private readonly DynamicService _dynamicService;

            private readonly IDeserializer _deserializer;

            private ILogger _logger = null!;

            private IConfiguration _configuration = null!;

            private Service _service = null!;

            public Configurator(DynamicService dynamicService)
            {
                _deserializer = new DeserializerBuilder().Build();

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

            public Service Configure()
            {
                _service.Validate(GetConfigurationVariables());

                return _service;
            }

            private Service ReadServiceDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, FileExtension.ServiceDefinition, SearchOption.AllDirectories)
                    .Take(1)
                    .Select(f =>
                    {
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
                    .EnumerateFiles(rootFolder, FileExtension.EntityDefinition, SearchOption.AllDirectories)
                    .Select(f =>
                    {
                        var entity = _deserializer.Deserialize<Entity>(ReadDefinitionFile(f));
                        entity.DefinitionFileName = Path.GetFullPath(f);
                        entity.Attributes.ToList().ForEach(a => { a.DefinitionFileName = Path.GetFullPath(f); });
                        return entity;
                    })
                    .ToList();
            }

            private List<Loader> ReadLoaderDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, FileExtension.LoaderDefinition, SearchOption.AllDirectories)
                    .Select(f =>
                    {
                        var loader = _deserializer.Deserialize<Loader>(ReadDefinitionFile(f));
                        loader.DefinitionFileName = Path.GetFullPath(f);
                        loader.Sources.ToList().ForEach(s => { s.DefinitionFileName = Path.GetFullPath(f); });
                        return loader;
                    })
                    .ToList();
            }

            private List<Api> ReadApiDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, FileExtension.ApiDefinition, SearchOption.AllDirectories)
                    .Select(f =>
                    {
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

            private IReadOnlyDictionary<string, string> GetConfigurationVariables()
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

                if (string.IsNullOrEmpty(_service.MessageBus.ConnectionString) && !
                    string.IsNullOrEmpty(_service.MessageBus.ConnectionVariable))
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
                    _service.Database
                };

                foreach (var kvp in _service.Loaders)
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