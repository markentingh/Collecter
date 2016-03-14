using System.Collections.Generic;

namespace Collector.Pages
{
    public class Download : Page
    {
        public Download() : base()
        {
        }

        public override string Render()
        {
            //setup page contents
            Scaffold scaffold = new Scaffold(S, "/app/pages/download.html", "", new string[] { });

            //get serverId from host name
            var host = S.Request.Host.ToString();
            var serverId = (int)S.Sql.ExecuteScalar("EXEC GetDownloadServerId @host='" + host + "'");
            if(serverId > 0)
            {
                var downloader = new Services.Downloads(S, S.Page.Url.paths);
                downloader.LoadDistributionList(serverId);
            }

            //render page
            scaffold.Data["script"] = S.Page.RenderJS();
            scaffold.Data["server-name"] = host;
            return scaffold.Render();
        }
    }
}
