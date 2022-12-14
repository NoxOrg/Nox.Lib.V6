using AutoMapper;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Models;
using Nox.Data;
using Nox.Etl;
using Nox.Messaging;

namespace Nox.Lib;

public static class ConfigurationExtensions
{
    public static MetaService ToMetaService(this INoxConfiguration source)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<NoxConfiguration, MetaService>();
            cfg.CreateMap<DatabaseConfiguration, ServiceDatabase>();
            cfg.CreateMap<ApiConfiguration, Api.Api>();
            cfg.CreateMap<ApiRouteConfiguration, Api.ApiRoute>();
            cfg.CreateMap<ApiRouteParameterConfiguration, Api.ApiRouteParameter>();
            cfg.CreateMap<ApiRouteResponseConfiguration, Api.ApiRouteResponse>();
            cfg.CreateMap<MessagingProviderConfiguration, MessagingProvider>();
            cfg.CreateMap<EntityConfiguration, Core.Models.Entity>();
            cfg.CreateMap<EntityAttributeConfiguration, EntityAttribute>();
            cfg.CreateMap<LoaderConfiguration, Loader>();
            cfg.CreateMap<LoaderScheduleConfiguration, LoaderSchedule>();
            cfg.CreateMap<LoaderScheduleRetryPolicyConfiguration, LoaderScheduleRetryPolicy>();
            cfg.CreateMap<LoaderLoadStrategyConfiguration, LoaderLoadStrategy>();
            cfg.CreateMap<LoaderTargetConfiguration, LoaderTarget>();
            cfg.CreateMap<LoaderMessageTargetConfiguration, LoaderMessageTarget>();
            cfg.CreateMap<LoaderSourceConfiguration, LoaderSource>();

        });
        var mapper = mapperConfig.CreateMapper();
        return mapper.Map<MetaService>(source);
    }
}