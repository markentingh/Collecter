namespace Collector.Includes
{
    public class Topics : Include
    {
        public Topics(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup feeds menu
            string menu = "<div class=\"menu left\"><nav><ul>" +
                "<li><a href=\"javascript:\" id=\"btnaddtopic\" class=\"button blue\">Add A Topic</a></li>" +
                "</ul></nav></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get feeds list from web service
                scaffold = new Scaffold(S, "/app/includes/dashboard/topics/list.html", "", new string[] { });
                Services.Feeds feeds = new Services.Feeds(S, S.Page.Url.paths);
                scaffold.Data["content"] = feeds.LoadFeedsUI();
                S.Page.RegisterJSFromFile("/app/includes/dashboard/topics/list.js");
            }
            return scaffold.Render();
        }

    }
}
