namespace Collector.Includes
{
    public class Feeds : Include
    {
        public Feeds(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup feeds menu
            string menu = "<div class=\"menu left\"><nav><ul>" +
                "<li><a href=\"javascript:\" id=\"btnaddfeed\" class=\"button blue\">Add Feed</a></li>" +
                "<li><a href=\"javascript:\" id=\"btncheckfeeds\" class=\"button green\">Check Feeds</a></li>" +
                "</ul></nav></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get feeds list from web service
                scaffold = new Scaffold(S, "/app/includes/dashboard/feeds/list.html", "", new string[] { "feeds" });
                Services.Feeds feeds = new Services.Feeds(S, S.Page.Url.paths);
                scaffold.Data["feeds"] = feeds.LoadFeedsUI();
                S.Page.RegisterJSFromFile("/app/includes/dashboard/feeds/list.js");
            }
            return scaffold.Render();
        }

    }
}
