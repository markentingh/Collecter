namespace Collector.PageViews
{
    public class Topics : PageView
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
            

            //setup container for topics section
            var scaffold2 = new Scaffold(S, "/app/pageviews/dashboard/topics/container.html");
            S.Page.RegisterJSFromFile("/app/pageviews/dashboard/topics/container.js");

            if (S.Page.Url.paths.Length > 2)
            {
                switch (S.Page.Url.paths[2].ToLower())
                {
                    case "edit":
                        //load topic editor
                        if (S.Request.Query.ContainsKey("topic"))
                        {
                            S.Page.RegisterJSFromFile("/app/pageviews/dashboard/topics/edit.js");
                            scaffold = new Scaffold(S, "/app/pageviews/dashboard/topics/edit.html");
                            Services.Topics topics = new Services.Topics(S, S.Page.Url.paths);
                            scaffold.Data["content"] = topics.LoadTopicsEditorUI(int.Parse(S.Request.Query["topic"]));
                            menu = "<div class=\"menu left\"><nav><ul>" +
                                        "<li><a href=\"javascript:\" id=\"btnaddsection\" class=\"button blue\">+ New Section</a></li>" +
                                    "</ul></nav></div>" +
                                    "<div class=\"menu right\"><nav><ul>" +
                                        "<li class=\"li-savechanges\" style=\"display:none;\"><a href=\"javascript:\" class=\"button green btn-savechanges\">Save Changes</a></li>" +
                                    "</ul></nav></div>";
                        }
                        break;
                }
            }
            if (scaffold == null)
            {
                //get topics list
                scaffold = new Scaffold(S, "/app/pageviews/dashboard/topics/list.html");
                Services.Topics topics = new Services.Topics(S, S.Page.Url.paths);
                scaffold.Data["content"] = topics.LoadTopicsUI();
                S.Page.RegisterJSFromFile("/app/pageviews/dashboard/topics/list.js");
            }
            parentScaffold.Data["menu"] = menu;
            scaffold2.Data["content"] = scaffold.Render();
            return scaffold2.Render();
        }

    }
}
