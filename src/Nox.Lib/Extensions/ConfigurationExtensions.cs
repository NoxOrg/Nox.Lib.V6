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
            
            //DataSource
            cfg.CreateMap<DataSourceConfiguration, ServiceDatabase>();
            
            //Api
            cfg.CreateMap<ApiConfiguration, Api.Api>();
            cfg.CreateMap<ApiRouteConfiguration, Api.ApiRoute>();
            cfg.CreateMap<ApiRouteParameterConfiguration, Api.ApiRouteParameter>();
            cfg.CreateMap<ApiRouteResponseConfiguration, Api.ApiRouteResponse>();
            
            //Messaging
            cfg.CreateMap<MessagingProviderConfiguration, MessagingProvider>();
            cfg.CreateMap<MessageTargetConfiguration, MessageTarget>();
            
            //Entity
            cfg.CreateMap<EntityConfiguration, Core.Models.Entity>();
            cfg.CreateMap<EntityAttributeConfiguration, EntityAttribute>();
            
            //Dto
            cfg.CreateMap<DtoConfiguration, Dto>();
            cfg.CreateMap<DtoAttributeConfiguration, DtoAttribute>();
            
            //Loader
            cfg.CreateMap<LoaderConfiguration, Loader>();
            cfg.CreateMap<LoaderScheduleConfiguration, LoaderSchedule>();
            cfg.CreateMap<LoaderScheduleRetryPolicyConfiguration, LoaderScheduleRetryPolicy>();
            cfg.CreateMap<LoaderLoadStrategyConfiguration, LoaderLoadStrategy>();
            cfg.CreateMap<LoaderTargetConfiguration, LoaderTarget>();
            cfg.CreateMap<LoaderSourceConfiguration, LoaderSource>();
            
            //Etl
            cfg.CreateMap<EtlConfiguration, Etl.Etl>();
            cfg.CreateMap<EtlSourcesConfiguration, EtlSources>();
            cfg.CreateMap<EtlRetryPolicyConfiguration, EtlRetryPolicy>();
            cfg.CreateMap<EtlScheduleConfiguration, EtlSchedule>();
            cfg.CreateMap<EtlDatabaseWatermarkConfiguration, EtlDatabaseWatermark>();
            cfg.CreateMap<EtlTransformConfiguration, EtlTransform>();
            cfg.CreateMap<EtlMapConfiguration, EtlMap>();
            cfg.CreateMap<EtlLookupConfiguration, EtlLookup>();
            cfg.CreateMap<EtlSourceMessageQueueConfiguration, EtlTargetMessageQueue>();
            cfg.CreateMap<EtlTargetFileConfiguration, EtlSourceFile>();
            cfg.CreateMap<EtlSourceDatabaseConfiguration, EtlSourceDatabase>();
            cfg.CreateMap<EtlSourceHttpConfiguration, EtlSourceHttp>();
            cfg.CreateMap<EtlHttpAuthConfiguration, EtlHttpAuth>();
            cfg.CreateMap<EtlTargetsConfiguration, EtlTargets>();
            cfg.CreateMap<EtlTargetDatabaseConfiguration, EtlTargetDatabase>();
        });
        var mapper = mapperConfig.CreateMapper();
        return mapper.Map<MetaService>(source);
    }
}