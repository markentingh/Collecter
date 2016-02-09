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
            string menu = "<div class=\"left\"><ul><li><a href=\"javascript:\" id=\"btnaddfeed\" class=\"button blue\">Add Feed</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get subjects list from web service
                scaffold = new Scaffold(S, "/app/includes/dashboard/feeds/list.html", "", new string[] { "feeds" });
                Services.Feeds feeds = new Services.Feeds(S, S.Page.Url.paths);
                scaffold.Data["feeds"] = feeds.LoadFeedsUI();
                S.Page.RegisterJSFromFile("/app/includes/dashboard/feeds/list.js");
            }
            return scaffold.Render();
        }

    }
}
