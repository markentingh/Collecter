using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Collector.Utility.DOM;


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
            //SqlClasses.Dashboard sqlDash;
            //SqlReader reader;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/feeds/add\" class=\"button blue\">Add Feed</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            //determine which section to load for articles
            if (S.Page.Url.paths.Length > 2)
            {
                switch (S.Page.Url.paths[2].ToLower())
                {
                    case "add":
                        
                        if(scaffold == null) { scaffold = LoadAddFeed(); }

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
                    scaffold = new Scaffold(S, "/app/includes/dashboard/feeds/list.html", "", new string[] { "content" });
                    S.Page.RegisterJSFromFile("/app/includes/dashboard/feeds/list.js");
                }
                
            }


            return scaffold.Render();
        }

        private Scaffold LoadAddFeed()
        {
            //load article creation form
            var scaffold = new Scaffold(S, "/app/includes/dashboard/feeds/add.html", "", new string[] { "content" });

            return scaffold;
        }

    }
}
