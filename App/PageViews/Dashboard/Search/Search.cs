using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Collector.Utility.DOM;


namespace Collector.PageViews
{
    public class Search : PageView
    {
        public Search(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/search/history\" class=\"button blue\">History</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            //determine which section to load for articles
            if (S.Page.Url.paths.Length > 2)
            {
                switch (S.Page.Url.paths[2].ToLower())
                {
                    case "history":
                        
                        if(scaffold == null) { scaffold = LoadHistory(); }

                        //render form
                        parentScaffold.Data["menu"] = "";
                        break;

                }
            }
            else
            {
                if (scaffold == null)
                {
                    //get article list from web service
                    scaffold = new Scaffold(S, "/app/pageviews/dashboard/search/search.html", "", new string[] { "content" });
                    S.Page.RegisterJSFromFile("/app/pageviews/dashboard/search/search.js");
                }
                
            }


            return scaffold.Render();
        }

        private Scaffold LoadHistory()
        {
            //load article creation form
            var scaffold = new Scaffold(S, "/app/pageviews/dashboard/search/history.html", "", new string[] { "content" });

            return scaffold;
        }

    }
}
