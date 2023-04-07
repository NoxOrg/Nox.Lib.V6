using AutoMapper;
using Nox.Core.Configuration;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Models;
using Nox.Data;
using Nox.Etl;
using Nox.Lib;
using Nox.Messaging;

namespace Nox;

public static class ConfigurationExtensions
{
    public static MetaService ToMetaService(this IProjectConfiguration source)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ProjectConfiguration, MetaService>();
            cfg.CreateMap<DataSourceConfiguration, ServiceDatabase>();
            cfg.CreateMap<ApiConfiguration, Api.Api>();
            cfg.CreateMap<ApiRouteConfiguration, Api.ApiRoute>();
            cfg.CreateMap<ApiRouteParameterConfiguration, Api.ApiRouteParameter>();
            cfg.CreateMap<ApiRouteResponseConfiguration, Api.ApiRouteResponse>();
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
        return mapper.Map<MetaService>(source);
    }
}