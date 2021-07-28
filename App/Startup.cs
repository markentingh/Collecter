using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Collector.SignalR.Hubs;
using Datasilk.Core.Extensions;

namespace Collector
{
    public class Startup
    {
        private static IConfigurationRoot config;
        private List<Assembly> assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };

        public virtual void ConfigureServices(IServiceCollection services)
        {
            //set up Server-side memory cache
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            //configure request form options
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            //add session
            services.AddSession();

            //add health checks
            services.AddHealthChecks();

            //add hsts
            services.AddHsts(options => { });
            services.AddHttpsRedirection(options => { });

            //add SignalR
            services.AddSignalR();

        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //get environment based on application build
            App.Environment = (env.EnvironmentName.ToLower()) switch
            {
                "production" => Environment.production,
                "staging" => Environment.staging,
                _ => Environment.development,
            };

            //load application-wide cache
            var configFile = "config" + (App.Environment == Environment.production ? ".prod" : "") + ".json";
            config = new ConfigurationBuilder()
                .AddJsonFile(App.MapPath(configFile))
                .AddEnvironmentVariables().Build();

            Server.Config = config;

            //configure Server defaults
            App.Host = config.GetSection("hostUrl").Value;
            if (config.GetSection("version").Value != null)
            {
                Server.Version = config.GetSection("version").Value;
            }

            //configure Server database connection strings
            Server.SqlActive = config.GetSection("sql:Active").Value;
            Server.SqlConnectionString = config.GetSection("sql:" + Server.SqlActive).Value;

            //configure Server security
            Server.BcryptWorkFactor = int.Parse(config.GetSection("Encryption:bcrypt_work_factor").Value);
            Server.Salt = config.GetSection("Encryption:salt").Value;

            //configure cookie-based authentication
            var expires = !string.IsNullOrEmpty(config.GetSection("Session:Expires").Value) ? int.Parse(config.GetSection("Session:Expires").Value) : 60;

            //use session
            var sessionOpts = new SessionOptions();
            sessionOpts.Cookie.Name = "Kandu";
            sessionOpts.IdleTimeout = TimeSpan.FromMinutes(expires);
            app.UseSession(sessionOpts);

            //handle static files
            var provider = new FileExtensionContentTypeProvider();

            // Add static file mappings
            provider.Mappings[".svg"] = "image/svg";
            var options = new StaticFileOptions
            {
                ContentTypeProvider = provider
            };
            app.UseStaticFiles(options);

            //exception handling
            if (App.Environment == Environment.development)
            {
                //app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions
                //{
                //    SourceCodeLineCount = 10
                //});
            }
            else
            {
                //use HTTPS
                app.UseHsts();
                app.UseHttpsRedirection();

                //use health checks
                app.UseHealthChecks("/health");
            }

            //use HTTPS
            //app.UseHttpsRedirection();

            //set up database connection
            Query.Sql.ConnectionString = Server.SqlConnectionString;
            Server.ResetPass = Query.Users.HasPasswords();
            Server.HasAdmin = Query.Users.HasAdmin();


            Server.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Cache.Add("browserPath", config.GetSection("browser:path").Value);

            //set up SignalR hubs
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ArticleHub>("/articlehub");
            });



            //run Datasilk application
            app.UseDatasilkMvc(new MvcOptions()
            {
                Routes = new Routes(),
                IgnoreRequestBodySize = true,
                WriteDebugInfoToConsole = false,
                LogRequests = false,
                InvokeNext = false
            });
        }
    }
}
