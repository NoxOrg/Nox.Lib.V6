using Microsoft.Extensions.Logging;
using Nox.Core.Constants;
using Nox.Core.Exceptions;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nox.Core.Configuration;

public class ProjectConfigurator
{
    private readonly string _designRoot;
    private readonly IDeserializer _deserializer;
    private ProjectConfiguration? _config;
    
    public ProjectConfigurator(string designRoot)
    {
        _designRoot = designRoot;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public ProjectConfiguration? LoadConfiguration()
    {
        _config = ReadServiceDefinition();
        if (_config == null) return _config;
        _config.Entities = ReadEntityDefinitionsFromFolder();
        _config.Loaders = ReadLoaderDefinitionsFromFolder();
        _config.Apis = ReadApiDefinitionsFromFolder();
        return _config;
    }

    private ProjectConfiguration? ReadServiceDefinition()
    {
        return Directory
            .EnumerateFiles(_designRoot, FileExtension.ServiceDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var config = _deserializer.Deserialize<ProjectConfiguration>(ReadDefinitionFile(f));
                config.DefinitionFileName = Path.GetFullPath(f);
                if (config.Database != null) config.Database.DefinitionFileName = Path.GetFullPath(f);
                if (config.MessagingProviders != null)
                {
                    foreach (var msgProviderConfig in config.MessagingProviders)
                    {
                        msgProviderConfig.DefinitionFileName = Path.GetFullPath(f);
                    }    
                }

                if (config.DataSources == null) return config;
                foreach (var dsConfig in config.DataSources)
                {
                    dsConfig.DefinitionFileName = Path.GetFullPath(f);
                }

                return config;
            })
            .FirstOrDefault();
    }
    
    private List<EntityConfiguration> ReadEntityDefinitionsFromFolder()
    {
        return Directory
            .EnumerateFiles(_designRoot, FileExtension.EntityDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var entity = _deserializer.Deserialize<EntityConfiguration>(ReadDefinitionFile(f));
                entity.DefinitionFileName = Path.GetFullPath(f);
                entity.Attributes.ToList().ForEach(a => { a.DefinitionFileName = Path.GetFullPath(f); });
                return entity;
            })
            .ToList();
    }
        
    private List<LoaderConfiguration> ReadLoaderDefinitionsFromFolder()
    {
        var loaders = Directory
            .EnumerateFiles(_designRoot, FileExtension.LoaderDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var loader = _deserializer.Deserialize<LoaderConfiguration>(ReadDefinitionFile(f));
                loader.DefinitionFileName = Path.GetFullPath(f);
                loader.Sources?.ToList().ForEach(s => { s.DefinitionFileName = Path.GetFullPath(f); });
                if (loader.LoadStrategy != null) loader.LoadStrategy.DefinitionFileName = Path.GetFullPath(f);
                loader.Messaging?.ToList().ForEach(m => { m.DefinitionFileName = Path.GetFullPath(f); });
                if (loader.Schedule != null) loader.Schedule.DefinitionFileName = Path.GetFullPath(f);
                if (loader.Target != null) loader.Target.DefinitionFileName = Path.GetFullPath(f);
                return loader;
            });
        return loaders.ToList();
    }
        
    private List<ApiConfiguration> ReadApiDefinitionsFromFolder()
    {
        return Directory
            .EnumerateFiles(_designRoot, FileExtension.ApiDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var api = _deserializer.Deserialize<ApiConfiguration>(ReadDefinitionFile(f));
                api.DefinitionFileName = Path.GetFullPath(f);
                return api;
            })
            .ToList();
    }
    
    private static string ReadDefinitionFile(string fileName)
    {
        Log.Information("Reading definition from {fileName}", fileName.Replace('\\', '/'));
        return File.ReadAllText(fileName);
    }
}