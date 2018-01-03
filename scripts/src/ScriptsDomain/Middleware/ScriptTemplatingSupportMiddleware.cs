using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ScriptsDomain.Middleware
{
    public class ScriptTemplatingSupportMiddleware
    {
        private IHostingEnvironment _hostingEnvironment;

        public ScriptTemplatingSupportMiddleware(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task Invoke(HttpContext context, IDictionary<Regex, Func<string, string>> mapper)
        {
            var fileInfo = _hostingEnvironment.WebRootFileProvider.GetFileInfo(context.Request.Path);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not found");
                return;
            }

            using (var outs = new StreamWriter(context.Response.Body))
            using (var ins = new StreamReader(fileInfo.CreateReadStream(), Encoding.UTF8))
            {
                var s = ins.ReadLine();
                while (s != null)
                {
                    foreach (var kvp in mapper)
                    {
                        var pattern = kvp.Key;
                        if (pattern.IsMatch(s))
                        {
                            var replacer = kvp.Value;
                            s = pattern.Replace(s, match => replacer(match.Value));
                        }
                    }

                    await outs.WriteLineAsync(s);
                    s = ins.ReadLine();
                }
            }
        }
    }
}