using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ScriptsDomain.Middleware
{
    public static class ScriptTemplatingSupportMiddlewareExtensions
    {
        private static readonly ConcurrentDictionary<string, IDictionary<Regex, Func<string, string>>> PathDependentMappers = new ConcurrentDictionary<string, IDictionary<Regex, Func<string, string>>>();

        public static void AddScriptTemplatingSupport(this IServiceCollection services)
        {
            services.AddSingleton<ScriptTemplatingSupportMiddleware>();
        }

        public static IApplicationBuilder UseScriptTemplatingSupport(this IApplicationBuilder builder, string scriptsBase, IDictionary<string, Func<string, string>> mapper)
        {
            var middleware = builder.ApplicationServices.GetService<ScriptTemplatingSupportMiddleware>();
            var regexMapper = PathDependentMappers.GetOrAdd(scriptsBase, s => CreateRegexMap(mapper));

            return builder.MapWhen(ctx => ctx.Request.Path.StartsWithSegments(scriptsBase), b => b.Run(ctx => middleware.Invoke(ctx, regexMapper)));
        }

        private static IDictionary<Regex, Func<string, string>> CreateRegexMap(IDictionary<string, Func<string, string>> mapper)
        {
            return mapper.ToDictionary(kvp => new Regex(@"\<%=\s*" + kvp.Key + @"\s%\>"), kvp => kvp.Value);
        }
    }
}