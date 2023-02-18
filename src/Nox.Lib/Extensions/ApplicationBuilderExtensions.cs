using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Core.Interfaces.Configuration;

namespace Nox;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        if (builder.ApplicationServices.GetService<IProjectConfiguration>() == null) return builder;
        builder.UseMiddleware<DynamicApiMiddleware>().UseRouting();

        builder.UseHangfireDashboard("/jobs");
        return builder;
    }
}