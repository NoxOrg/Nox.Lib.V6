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

        private Service _service = null!;

        public void ValidateDatabaseSchema()
        {
            if (_service.Database is not null)
            {
                SqlServerMigrationProvider.ValidateDatabaseSchema(_service.Database, _service.Entities);
            }
        }

        public void ExecuteDataLoaders()
        {
            if (_service.Database is not null)
            {
                var loaderProvider = new SqlServerLoaderProvider(_configuration);

                loaderProvider.ExecuteLoaders(_service.Database, _service.Loaders, _service.Entities);
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

            public DynamicService Build()
            {
                return _dynamicService;
            }


            private Service ReadServiceDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, SERVICE_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Take(1)
                    .Select(f => _deserializer.Deserialize<Service>(File.ReadAllText(f)))
                    .First();
            }

            private List<Entity> ReadEntityDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, ENTITITY_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<Entity>(File.ReadAllText(f)))
                    .ToList();
            }

            private List<Loader> ReadLoaderDefinitionsFromFolder(string rootFolder)
            {
                return Directory
                    .EnumerateFiles(rootFolder, LOADER_DEFINITION_PATTERN, SearchOption.AllDirectories)
                    .Select(f => _deserializer.Deserialize<Loader>(File.ReadAllText(f)))
                    .ToList();
            }

        }


    }
}