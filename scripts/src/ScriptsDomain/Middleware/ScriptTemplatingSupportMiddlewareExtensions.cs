using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace ScriptsDomain.Middleware
{
    public static class ScriptTemplatingSupportMiddlewareExtensions
    {
        public static IApplicationBuilder UseScriptTemplatingSupport(this IApplicationBuilder builder, string scriptsBase, IDictionary<string, Func<string, string>> mapper)
        {
            return builder.UseMiddleware<ScriptTemplatingSupportMiddleware>(builder, scriptsBase, mapper);
        }
    }
}