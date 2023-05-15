using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Exceptions;
using Nox.Core.Helpers;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Database;
using Nox.Core.Models;
using Nox.Core.Validation.Configuration;

namespace Nox.Core.Extensions;

public static class ServiceCollectionExtensions
{

    private static IProjectConfiguration? _projectConfig;

    public static IProjectConfiguration? NoxProjectConfiguration => _projectConfig;

    public static IServiceCollection AddNoxConfiguration(this IServiceCollection services, string? designRoot = null)
    {
        var configuration = ConfigurationHelper.GetNoxAppSettings();

        if (configuration != null && designRoot == null) 
        { 
            designRoot = configuration["Nox:DesignFolder"] ?? configuration["Nox:DefinitionRootPath"];
        }

        if (!Directory.Exists(designRoot))
        {
            designRoot = "./";
        }

        if (designRoot == null)
        {
            throw new ConfigurationException("Could not load Nox configuration.");
        }

        var configurator = new ProjectConfigurator(designRoot);

        var yamlConfig = configurator.LoadConfiguration();
        
        if (yamlConfig != null)
        {
            var validator = new YamlConfigValidator();
            validator.ValidateAndThrow(yamlConfig);

            _projectConfig = new Configurator(yamlConfig)
                .WithAppConfiguration(configuration!)
                .Configure();

            services.AddSingleton(_projectConfig);
        }

        return services;
    }

    public static bool ConfirmNoxConfigurationAdded(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(IProjectConfiguration));
    }

    private class Configurator
    {
        private IProjectConfiguration? _projectConfig;
        private IConfiguration? _appConfig;
        private readonly IYamlConfiguration? _yamlConfig;

        public Configurator(IYamlConfiguration projectConfig)
        {
            _yamlConfig = projectConfig;
            _projectConfig = _yamlConfig.ToMetaService();
        }

        public Configurator WithAppConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
            return this;
        }

        public IProjectConfiguration Configure()
        {
            _projectConfig = _yamlConfig!.ToMetaService();

            var serviceDatabases = GetServiceDatabasesFromNoxConfig();

            ResolveConfigurationVariables(serviceDatabases);

            _projectConfig.Validate();

            _projectConfig.Configure();

            return _projectConfig;
        }


        private void ResolveConfigurationVariables(IList<IServiceDataSource> serviceDatabases)
        {
            // populate variables from application config
            var databases = serviceDatabases
                .Where(d => string.IsNullOrEmpty(d.ConnectionString))
                .Where(d => !string.IsNullOrEmpty(d.ConnectionVariable));

            var variables = databases
                .Select(d => d.ConnectionVariable!)
                .ToHashSet()
                .ToDictionary(v => v, v => _appConfig?[v], StringComparer.OrdinalIgnoreCase);

            if (_projectConfig!.MessagingProviders is { Count: > 0 })
            {
                foreach (var item in _projectConfig.MessagingProviders)
                {
                    if (string.IsNullOrEmpty(item.ConnectionString) && !string.IsNullOrEmpty(item.ConnectionVariable))
                    {
                        variables.Add(item.ConnectionVariable, _appConfig?[item.ConnectionVariable]);
                    }
                }
            }

            // try key vault where app configuration is missing 
            if (variables.Any(v => v.Value == null))
            {
                // TryAddMissingConfigsFromKeyVault(_metaService.KeyVaultUri, variables!);
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

            if (_projectConfig.MessagingProviders is { Count: > 0 })
            {
                foreach (var item in _projectConfig.MessagingProviders)
                {
                    if (string.IsNullOrEmpty(item.ConnectionString) && !string.IsNullOrEmpty(item.ConnectionVariable))
                    {
                        item.ConnectionString = variables[item.ConnectionVariable!];
                    }
                }
            }
        }

        /*
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
                return;
            }

            foreach (var key in variables.Keys)
            {

                if (string.IsNullOrEmpty(variables[key]))
                {
                    variables[key] = keyVault.GetSecretAsync(vaultUri, key.Replace(":", "--")).GetAwaiter().GetResult().Value;
                }

            }
        }

        */

        private IList<IServiceDataSource> GetServiceDatabasesFromNoxConfig()
        {
            var serviceDatabases = new List<IServiceDataSource>
            {
                _projectConfig!.Database!
            };

            foreach (var dataSource in _projectConfig.DataSources!)
            {
                serviceDatabases.Add(dataSource);
            }

            return serviceDatabases;
        }

    }



}