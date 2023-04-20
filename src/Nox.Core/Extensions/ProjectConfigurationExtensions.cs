using AutoMapper;
using Nox.Core.Configuration;
using Nox.Core.Configuration.Secrets;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Models;

namespace Nox.Core.Extensions;

public static class ProjectConfigurationExtensions
{
    internal static Models.ProjectConfiguration ToMetaService(this IYamlConfiguration source)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<YamlConfiguration, ProjectConfiguration>();
            cfg.CreateMap<SecretConfiguration, Secret>();
            cfg.CreateMap<DataSourceConfiguration, ServiceDatabase>();
            cfg.CreateMap<ApiConfiguration, Core.Models.Api>();
            cfg.CreateMap<ApiRouteConfiguration, Core.Models.ApiRoute>();
            cfg.CreateMap<ApiRouteParameterConfiguration, ApiRouteParameter>();
            cfg.CreateMap<ApiRouteResponseConfiguration, ApiRouteResponse>();
            cfg.CreateMap<MessagingProviderConfiguration, MessagingProvider>();
            cfg.CreateMap<EntityConfiguration, Core.Models.Entity>();
            cfg.CreateMap<EntityRelationshipConfiguration, EntityRelationship>();
            cfg.CreateMap<EntityAttributeConfiguration, EntityAttribute>();
            cfg.CreateMap<EntityKeyConfiguration, EntityKey>();
            cfg.CreateMap<MessageTargetConfiguration, MessageTarget>();
            cfg.CreateMap<LoaderConfiguration, Loader>();
            cfg.CreateMap<LoaderScheduleConfiguration, LoaderSchedule>();
            cfg.CreateMap<LoaderScheduleRetryPolicyConfiguration, LoaderScheduleRetryPolicy>();
            cfg.CreateMap<LoaderLoadStrategyConfiguration, LoaderLoadStrategy>();
            cfg.CreateMap<LoaderTargetConfiguration, LoaderTarget>();
            cfg.CreateMap<MessageTargetConfiguration, MessageTarget>();
            cfg.CreateMap<LoaderSourceConfiguration, LoaderSource>();
        });

        var mapper = mapperConfig.CreateMapper();
        return mapper.Map<Models.ProjectConfiguration>(source);
    }

}