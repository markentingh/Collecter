using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class Startup : Datasilk.Startup
{

    public override void Configured(IApplicationBuilder app, IHostingEnvironment env, IConfigurationRoot config)
    {
        base.Configured(app, env, config);
        Query.QuerySql.connectionString = server.sqlConnectionString;
        var query = new Collector.Query.Users();
        var resetPass = query.HasPasswords();
        server.hasAdmin = query.HasAdmin();
    }
}
