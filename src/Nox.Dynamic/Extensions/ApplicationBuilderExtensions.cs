using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Nox.Dynamic.Extensions;

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
