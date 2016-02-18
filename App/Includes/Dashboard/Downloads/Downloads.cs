namespace Collector.Includes
{
    public class Downloads : Include
    {
        public Downloads(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup feeds menu
            string menu = "<div class=\"menu left\"><nav><ul>" +
                "<li><a href=\"javascript:\" id=\"btnaddserver\" class=\"button blue\">Add Server</a></li>" +
                "<li><a href=\"javascript:\" id=\"btnanalyze\" class=\"button\">Download Articles</a></li>" +
                "</ul></nav></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get feeds list from web service
                scaffold = new Scaffold(S, "/app/includes/dashboard/downloads/downloads.html", "", new string[] { });
                Services.Feeds feeds = new Services.Feeds(S, S.Page.Url.paths);
                scaffold.Data["feeds"] = feeds.LoadFeedsUI();
                S.Page.RegisterJSFromFile("/app/includes/dashboard/downloads/downloads.js");
            }
            return scaffold.Render();
        }

    }
}
