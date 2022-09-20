using Microsoft.Extensions.Configuration;
using Nox.Dynamic.Dto;
using Nox.Dynamic.Loaders.Providers;
using Nox.Dynamic.Migrations.Providers;
using YamlDotNet.Serialization;

namespace Nox.Dynamic
{
    public class DynamicService
    {

        private IConfiguration _configuration = null!;

        private ServiceDefinition _service = null!;

        private EntityDefinition[] _entities = null!;

        private LoaderDefinition[] _loaders = null!;

        public void ValidateDatabaseSchema()
        {
            if (_service.Database is not null)
            {
                SqlServerMigrationProvider.ValidateDatabaseSchema(_service.Database, _entities);
            }
        }

        public void ExecuteDataLoaders()
        {
            if (_service.Database is not null)
            {
                var loaderProvider = new SqlServerLoaderProvider(_configuration);

                loaderProvider.ExecuteLoaders(_service.Database, _loaders);
            }
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

            public Builder()
            {
                _deserializer = new DeserializerBuilder().Build();

                _dynamicService = new DynamicService();
            }

            public Builder FromRootFolder(string rootFolder)
            {
                _dynamicService._service = ReadServiceDefinitionsFromFolder(rootFolder);

                _dynamicService._entities = ReadEntityDefinitionsFromFolder(rootFolder);

                _dynamicService._loaders = ReadLoaderDefinitionsFromFolder(rootFolder);

                return this;
            }

            public Builder WithConfiguration(IConfiguration configuration)
            {
                _dynamicService._configuration = configuration;

                return this;
            }

            public DynamicService Build()
            {
                return _dynamicService;
            }


            private ServiceDefinition ReadServiceDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, SERVICE_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Take(1)
                    .Select(f => _deserializer.Deserialize<ServiceDefinition>(File.ReadAllText(f)))
                    .First();
            }

            private EntityDefinition[] ReadEntityDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, ENTITITY_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<EntityDefinition>(File.ReadAllText(f)))
                    .ToArray();
            }

            private LoaderDefinition[] ReadLoaderDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, LOADER_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<LoaderDefinition>(File.ReadAllText(f)))
                    .ToArray();
            }

        }


    }
}