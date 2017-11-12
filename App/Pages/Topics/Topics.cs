using System;
using System.Collections.Generic;
using System.Linq;

namespace Collector.Pages
{
    public class Topics : Page
    {
        public Topics(Core CollectorCore) : base(CollectorCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load dashboard interface
            var dashboard = new Partials.Dashboard(S);

            //add custom menu to dashboard
            dashboard.AddMenuItem("btnaddtopic", "Add A Topic", "");

            Scaffold scaffold = null;

            if(path.Length > 1)
            {
                if(path[1].ToLower() == "edit")
                {

                }

            }

            if(scaffold == null)
            {
                //load topics scaffold HTML
                scaffold = new Scaffold(S, "/Pages/Topics/topics.html");

                //load topics list
                var topics = new Services.Topics(S);
                var inject = topics.InjectTopicsUI();
                scaffold.Data["content"] = inject.html;
                if (scaffold.Data["content"] == Error())
                {
                    scaffold.Data["content"] = "";
                    scaffold.Data["no-topics"] = "1";
                }
            }
            

            //add page resources
            dashboard.AddScript("/js/pages/topics/topics.js");
            dashboard.AddCSS("/css/pages/topics/topics.css");

            //finally, render page
            return dashboard.Render(path, scaffold.Render(), metadata);
        }
    }
}
