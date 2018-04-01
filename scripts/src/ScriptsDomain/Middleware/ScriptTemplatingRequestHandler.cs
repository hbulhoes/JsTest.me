using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ScriptsDomain.Middleware
{
    public class ScriptTemplatingRequestHandler
    {
        private readonly Dictionary<Regex, Func<string, HttpContext, string>> _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ScriptTemplatingRequestHandler(Dictionary<string, Func<string, HttpContext, string>> mapper, IHostingEnvironment hostingEnvironment)
        {
            _mapper = mapper.ToDictionary(kvp => new Regex(@"\<%=\s*" + kvp.Key + @"\s%\>"), kvp => kvp.Value);
            _hostingEnvironment = hostingEnvironment;
        }

        public void HandleQuery(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var content = TransformFileTemplate(context.Request.Path, context);
                if (content == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not found");
                    return;
                }

                await context.Response.WriteAsync(content);
            });
        }

        private string TransformFileTemplate(PathString requestPath, HttpContext context)
        {
            var fileInfo = _hostingEnvironment.WebRootFileProvider.GetFileInfo(context.Request.Path);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                return null;
            }

            var outs = new StringBuilder();

            using (var ins = new StreamReader(fileInfo.CreateReadStream(), Encoding.UTF8))
            {
                var s = ins.ReadLine();
                while (s != null)
                {
                    foreach (var kvp in _mapper)
                    {
                        var pattern = kvp.Key;
                        if (pattern.IsMatch(s))
                        {
                            var replacer = kvp.Value;
                            s = pattern.Replace(s, match => replacer(match.Value, context));
                        }
                    }

                    outs.AppendLine(s);
                    s = ins.ReadLine();
                }
            }

            return outs.ToString();
        }
    }
}