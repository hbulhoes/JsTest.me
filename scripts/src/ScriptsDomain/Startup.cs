using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScriptsDomain.Middleware;

namespace ScriptsDomain
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddScriptTemplatingSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseDefaultFiles();
            //app.UseStaticFiles(); // com esta linha des-comentada, o MapWhen logo abaixo deixará de funcionar.

            var handler = new ScriptTemplatingRequestHandler(
                new Dictionary<string, Func<string, HttpContext, string>>
                {
                    {"cdnDomain", (s, ctx) =>
                        {
                            var headers = ctx.Request.Headers;
                            return headers.ContainsKey("Domain") ? headers["Domain"] : headers["Host"];
                        }
                    }
                },
                app.ApplicationServices.GetService<IHostingEnvironment>());
            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/scripts"), handler.HandleQuery);
        }
    }
}
