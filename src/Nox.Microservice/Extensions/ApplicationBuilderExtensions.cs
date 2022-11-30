using Hangfire;
using Microsoft.AspNetCore.Builder;
using Nox.Api;

namespace Nox.Microservice.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        builder.UseMiddleware<DynamicApiMiddleware>().UseRouting();

        builder.UseHangfireDashboard("/jobs");

        return builder;
    }
}