using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ScriptsDomain.Middleware
{
    public class ScriptTemplatingSupportMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PathString _scriptsBase;
        private readonly IDictionary<Regex, Func<string, string>> _mapper;

        public ScriptTemplatingSupportMiddleware(RequestDelegate next, IApplicationBuilder builder, string scriptsBase, IDictionary<string, Func<string, string>> mapper)
        {
            _next = next;
            _scriptsBase = scriptsBase;
            _mapper = mapper.ToDictionary(kvp => new Regex(@"\<%=\s*" + kvp.Key + @"\s%\>"), kvp => kvp.Value);
        }

        public async Task Invoke(HttpContext context)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(context);

            if (context.Response.StatusCode == 200 && context.Request.Path.StartsWithSegments(_scriptsBase))
            {
                var outs = new StringWriter(new StringBuilder());
                using (var ins = new StreamReader(context.Response.Body, Encoding.UTF8))
                {
                    var s = ins.ReadLine();
                    foreach (var kvp in _mapper)
                    {
                        var pattern = kvp.Key;
                        if (pattern.IsMatch(s))
                        {
                            var replacer = kvp.Value;
                            s = pattern.Replace(s, match => replacer(match.Value));
                        }
                    }

                    outs.WriteLine(s);
                }
            }
        }
    }
}