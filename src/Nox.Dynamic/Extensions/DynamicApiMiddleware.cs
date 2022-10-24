using Google.Protobuf.WellKnownTypes;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Nox.Dynamic.MetaData;
using Nox.Dynamic.OData.Models;
using System.Dynamic;
using System.Text.Json;

namespace Nox.Dynamic.Extensions;

public class DynamicApiMiddleware
{
    private readonly PathString DYNAMIC_API_PREFIX = "/api";

    private const Int32 BACKSLASH = 92;

    private readonly RequestDelegate _next;

    private readonly List<RouteMatcher> _matchers = new();

    public DynamicApiMiddleware(RequestDelegate next, DynamicModel model)
    {
        _next = next;

        foreach (var api in model.Configuration.Apis.Values)
        {
            foreach (var route in api.Routes)
            {
                if (route.Name[0] != BACKSLASH)
                {
                    route.Name = @$"/{route.Name}";
                }

                if (route.TargetUrl[0] != BACKSLASH)
                {
                    route.TargetUrl = @$"/{route.TargetUrl}";
                }

                _matchers.Add(new RouteMatcher(route, DYNAMIC_API_PREFIX));
            }
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        if (!path.HasValue)
        {
            await _next(context);
            return;
        }

        if (!path.StartsWithSegments(DYNAMIC_API_PREFIX))
        {
            await _next(context);
            return;
        }

        RouteValueDictionary? urlParameters = null;

        ApiRoute? apiRoute = null;

        foreach (var matcher in _matchers)
        {
            urlParameters = matcher.Match(path);

            if (urlParameters != null)
            {
                apiRoute = matcher.ApiRoute;
                break;
            }
        }

        if (urlParameters == null || apiRoute == null)
        {
            await _next(context);
            return;
        }

        var translatedTarget = apiRoute.TargetUrl;


        foreach (var kvp in context.Request.Query)
        {
            translatedTarget = translatedTarget.Replace($"{{{kvp.Key}}}", kvp.Value.ToString());
        }

        foreach (var kvp in urlParameters)
        {
            translatedTarget = translatedTarget.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString());
        }

        foreach (var parameter in apiRoute.Parameters)
        {
            translatedTarget = translatedTarget.Replace($"{{{parameter.Name}}}", parameter.Default?.ToString());
        }

        var parts = translatedTarget.Split('?',2);

        context.Request.Path = new PathString(parts[0]);

        if (parts.Length > 1)
        {
            context.Request.QueryString = new QueryString("?" + parts[1]);
        }

        await _next(context);
    }

}

public static class RequestNoxMiddlewareExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        builder.UseMiddleware<DynamicApiMiddleware>().UseRouting();

        builder.UseHangfireDashboard("/cron");

        return builder;
    }
}

internal class RouteMatcher
{

    private readonly string _template;

    private readonly TemplateMatcher _matcher;

    private readonly ApiRoute _apiRoute;

    public RouteMatcher(ApiRoute apiRoute, string prefix)
    {
        _template = $"{prefix}{apiRoute.Name}";

        var template = TemplateParser.Parse(_template);

        _apiRoute = apiRoute;

        _matcher = new TemplateMatcher(template, GetDefaults(template));
    }

    public ApiRoute ApiRoute => _apiRoute;

    public RouteValueDictionary? Match(string requestPath)
    {

        var result = new RouteValueDictionary();

        if(_matcher.TryMatch(requestPath, result))
        { 
           return result;
        }

        return null;

    }

    private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
    {
        var result = new RouteValueDictionary();

        foreach (var parameter in parsedTemplate.Parameters)
        {
            if (parameter.DefaultValue != null)
            {
                result.Add(parameter.Name!, parameter.DefaultValue);
            }
        }

        return result;
    }
}