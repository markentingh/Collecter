using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Collector
{

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCaching();
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIISPlatformHandler();

            //load application-wide memory store
            Server server = new Server(app, env);

            //handle static files
            var options = new StaticFileOptions {ContentTypeProvider = new FileExtensionContentTypeProvider()};
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.Add( new KeyValuePair<string, string>(".less", "text/css"));
            app.UseStaticFiles(options);

            //exception handling
            var errOptions = new ErrorPageOptions();
            errOptions.SourceCodeLineCount = 10;
            app.UseDeveloperExceptionPage();


            //use session (3 hour timeout)
            app.UseSession();

            //get server info from config.json
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(server.MapPath("config.json"))
                .AddEnvironmentVariables();
            IConfiguration config = configBuilder.Build();

            string active = config.GetSection("Data:Active").Value;
            string conn = config.GetSection("Data:" + active).Value;
            server.sqlActive = active; 
            server.sqlConnection = conn;

            server.analyzerVersion = config.GetSection("analyzer:version").Value;

            //run application
            app.Run(async (context) =>
            {
                var strings = new Utility.Str(null);
                var requestStart = DateTime.Now;
                DateTime requestEnd;
                TimeSpan tspan;
                var path = context.Request.Path.ToString();
                var paths = path.Split("/"[0]).Skip(1).ToArray();
                string requestType = "";
                var extension = strings.getFileExtension(path);
                server.requestCount += 1;
                Console.WriteLine("--------------------------------------------");
                Console.WriteLine("{0} GET {1}", DateTime.Now.ToString("hh:mm:ss"), context.Request.Path);

                if (paths.Length > 1)
                {
                    if(paths[0]=="api")
                    {
                        //run a web service via ajax (e.g. /collector/namespace/class/function)
                         IFormCollection form = null;
                        if(context.Request.ContentType != null)
                        {
                            if (context.Request.ContentType.IndexOf("application/x-www-form-urlencoded") >= 0)
                            {
                            }else if (context.Request.ContentType.IndexOf("multipart/form-data") >= 0)
                            {
                                //get files collection from form data
                                form = await context.Request.ReadFormAsync();
                            }
                        }
                        
                        //start the Web Service engine
                        var ws = new Pipeline.WebService(server, context, paths, form);
                        requestType = "service";
                    }
                }

                if(requestType == "" && extension == "")
                {
                    //initial page request
                    var r = new Pipeline.App(server, context, paths);
                    requestType = "page";
                }

                if(requestType == "" && extension != "")
                {
                    //file
                    requestType = "file";
                    //404 file not found
                }

                if(requestType == "") {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("Collector - an email template builder.");
                }

                requestEnd = DateTime.Now;
                tspan = requestEnd - requestStart;
                server.requestTime += (tspan.Seconds);
                Console.WriteLine("END GET {0} {1} ms {2}", context.Request.Path, tspan.Milliseconds, requestType);
                Console.WriteLine("");
            });
        }
    }
}
