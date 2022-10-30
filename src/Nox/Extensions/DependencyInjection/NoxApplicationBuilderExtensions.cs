using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Nox.Extensions.DependencyInjection;

public static class NoxApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        builder.UseMiddleware<DynamicApiMiddleware>().UseRouting();

        builder.UseHangfireDashboard("/jobs");

        return builder;
    }
}
