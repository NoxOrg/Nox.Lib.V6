using System.Collections;
using Nox.Core.Configuration;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nox.Core.Helpers;

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
        _config.Etls = ReadEtlDefinitionsFromFolder();
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
                SetDefinitionFilename(config, Path.GetFullPath(f));
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
                SetDefinitionFilename(entity, Path.GetFullPath(f));
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
                SetDefinitionFilename(loader, Path.GetFullPath(f));
                return loader;
            });
        return loaders.ToList();
    }

    private List<EtlConfiguration> ReadEtlDefinitionsFromFolder()
    {
        var loaders = Directory
            .EnumerateFiles(_designRoot, FileExtension.EtlDefinition, SearchOption.AllDirectories)
            .Select(f =>
            {
                var etl = _deserializer.Deserialize<EtlConfiguration>(ReadDefinitionFile(f));
                SetDefinitionFilename(etl, Path.GetFullPath(f));
                return etl;
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
                SetDefinitionFilename(api, Path.GetFullPath(f));
                return api;
            })
            .ToList();
    }
    
    private static string ReadDefinitionFile(string fileName)
    {
        Log.Information("Reading definition from {fileName}", fileName.Replace('\\', '/'));
        return File.ReadAllText(fileName);
    }

    private static void SetDefinitionFilename(IMetaBase metaBase, string path)
    {
        metaBase.DefinitionFileName = path;
        foreach (var prop in metaBase.GetType().GetProperties())
        {
            if (prop.PropertyType.IsAssignableTo(typeof(IMetaBase)))
            {
                var value = (IMetaBase)prop.GetValue(metaBase);
                if (value != null) SetDefinitionFilename(value, path);
            }
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericArguments().Any(a => a.IsAssignableTo(typeof(IMetaBase))))
            {
                var listValue = prop.GetValue(metaBase);
                if (listValue == null) continue;
                foreach (var item in (listValue as IEnumerable)!)
                {
                    SetDefinitionFilename((IMetaBase)item!, path);
                }
            }
        }
    }
}