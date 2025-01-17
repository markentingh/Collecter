﻿using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Collector
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                .UseKestrel(
                    options =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                    }
                )
                .UseStartup<Startup>();
            });


        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}