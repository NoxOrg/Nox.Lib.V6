using AutoMapper;
using Nox.Core.Configuration;
using Nox.Core.Configuration.Secrets;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Models;
using Nox.Core.Models.Entity;

namespace Nox.Core.Extensions;

public static class ProjectConfigurationExtensions
{
    internal static IProjectConfiguration ToMetaService(this IYamlConfiguration source)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<YamlConfiguration, ProjectConfiguration>();
            cfg.CreateMap<SecretConfiguration, Secret>();
            cfg.CreateMap<DataSourceConfiguration, ServiceDatabase>();
            cfg.CreateMap<ApiConfiguration, Api>();
            cfg.CreateMap<ApiRouteConfiguration, ApiRoute>();
            cfg.CreateMap<ApiRouteParameterConfiguration, ApiRouteParameter>();
            cfg.CreateMap<ApiRouteResponseConfiguration, ApiRouteResponse>();
            cfg.CreateMap<MessagingProviderConfiguration, MessagingProvider>();
            cfg.CreateMap<EntityConfiguration, Entity>();
            cfg.CreateMap<EntityRelationshipConfiguration, EntityRelationship>();
            cfg.CreateMap<OwnedEntityRelationshipConfiguration, OwnedEntityRelationship>();
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
            cfg.CreateMap<VersionControlConfiguration, VersionControl>();
            cfg.CreateMap<TeamConfiguration, Team>();
            cfg.CreateMap<TeamMemberConfiguration, TeamMember>();
            cfg.CreateMap<ParameterConfiguration, QueryParameter>();
            cfg.CreateMap<QueryConfiguration, EntityQuery>();
            cfg.CreateMap<CommandConfiguration, EntityCommand>();
            cfg.CreateMap<ResponseConfiguration, QueryResponse>();
        });

        var mapper = mapperConfig.CreateMapper();
        return mapper.Map<ProjectConfiguration>(source);
    }
}