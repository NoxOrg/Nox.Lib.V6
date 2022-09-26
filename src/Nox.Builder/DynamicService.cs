using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Dto;
using Nox.Dynamic.Loaders.Providers;
using Nox.Dynamic.Migrations.Providers;
using YamlDotNet.Serialization;

namespace Nox.Dynamic
{
    public class DynamicService
    {

        private IConfiguration _configuration = null!;

        private ILogger _logger = null!;

        private Service _service = null!;

        public async Task<bool> ValidateDatabaseSchemaAsync()
        {
            _logger.LogInformation("Validating the database schema");

            if (_service.Database is not null)
            {
                var migrationProvider = new SqlServerMigrationProvider(_configuration, _logger);

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
                var loaderProvider = new SqlServerLoaderProvider(_configuration, _logger);

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

            private string ReadDefinitionFile(string fileName)
            {
                _logger.LogInformation("Reading definition from {fileName}", fileName.Replace('\\','/'));

                return File.ReadAllText(fileName);
            }

        }


    }
}