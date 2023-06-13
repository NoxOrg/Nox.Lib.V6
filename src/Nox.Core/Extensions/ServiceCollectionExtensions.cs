using Microsoft.Extensions.DependencyInjection;
using Nox.Solution;

namespace Nox.Core.Extensions;

public static class ServiceCollectionExtensions
{

    private static NoxSolution? _solution;

    public static NoxSolution? Solution => _solution;

    public static IServiceCollection AddNoxSolution(this IServiceCollection services)
    {
        _solution = new NoxSolutionBuilder()
            .UseDependencyInjection(services)
            .Build();
        return services;
    }

    public static bool ConfirmNoxConfigurationAdded(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(NoxSolution));
    }
}