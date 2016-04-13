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

            //setup topics menu
            string menu = "<div class=\"menu left\"><nav><ul>" +
                "<li><a href=\"javascript:\" id=\"btnaddtopic\" class=\"button blue\">Add A Topic</a></li>" +
                "</ul></nav></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get topics list
                scaffold = new Scaffold(S, "/app/includes/dashboard/topics/list.html", "", new string[] { });
                Services.Topics topics = new Services.Topics(S, S.Page.Url.paths);
                scaffold.Data["content"] = topics.LoadTopicsUI();
                S.Page.RegisterJSFromFile("/app/includes/dashboard/topics/list.js");
            }
            return scaffold.Render();
        }

    }
}
