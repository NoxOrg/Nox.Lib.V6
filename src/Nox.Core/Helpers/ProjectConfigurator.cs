using Nox.Core.Configuration;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Models;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nox.Core.Helpers;

public class ProjectConfigurator
{
    private readonly string _designRoot;
    private readonly IDeserializer _deserializer;
    private YamlConfiguration? _config;
    
    public ProjectConfigurator(string designRoot)
    {
        _designRoot = designRoot;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    internal YamlConfiguration? LoadConfiguration()
    {
        _config = ReadServiceDefinition();
        if (_config == null) return _config;
        _config.Entities = ReadEntityDefinitionsFromFolder();
        _config.Loaders = ReadLoaderDefinitionsFromFolder();
        _config.Apis = ReadApiDefinitionsFromFolder();
        return _config;
    }

    private YamlConfiguration? ReadServiceDefinition()
    {
        return Directory
            .EnumerateFiles(_designRoot, FileExtension.ServiceDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var definitionFullPath = Path.GetFullPath(f);
                var config = _deserializer.Deserialize<YamlConfiguration>(ReadDefinitionFile(f));
                config.DefinitionFileName = Path.GetFullPath(f);
                if (config.Database != null) config.Database.DefinitionFileName = Path.GetFullPath(f);
                if (config.MessagingProviders != null)
                {
                    foreach (var msgProviderConfig in config.MessagingProviders)
                    {
                        msgProviderConfig.DefinitionFileName = definitionFullPath;
                    }    
                }

                if (config.DataSources != null)
                {
                    foreach (var dsConfig in config.DataSources)
                    {
                        dsConfig.DefinitionFileName = definitionFullPath;
                    }
                }

                if (config.VersionControl != null)
                {
                    config.VersionControl.DefinitionFileName = definitionFullPath;
                }

                if (config.Team != null)
                {
                    config.Team.DefinitionFileName = definitionFullPath;
                    foreach (var dev in config.Team.Developers)
                    {
                        dev.DefinitionFileName = definitionFullPath;
                    }
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