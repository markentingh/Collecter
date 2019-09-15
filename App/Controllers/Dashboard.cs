using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Collector.Partials
{
    public class Dashboard : Controller
    {
        private struct menuItem
        {
            public string title;
            public string url;
            public string id;
        }

        private List<menuItem> menuItems = new List<menuItem>();

        public Dashboard(HttpContext context, Parameters parameters) : base(context, parameters) { }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            //check security first
            if (CheckSecurity() == false) { return AccessDenied(); }

            //setup scaffolding variables
            Scaffold scaffold = new Scaffold("/Views/Dashboard/dashboard.html");

            //set title
            scaffold["title"] = "Collector";

            //load body
            scaffold["content"] = body;

            //load custom menu
            if(menuItems.Count > 0)
            {
                scaffold["menu"] = "<ul class=\"tabs right\">" +
                    string.Join("", 
                        menuItems.Select<menuItem, string>((menuItem item) =>
                        {
                            return "<li class=\"pad-left\"><a href=\"" + (item.url != "" ? item.url : "javascript:") + "\"" +
                                    (item.id != "" ? " id=\"" + item.id + "\"" : "") +
                                    " class=\"button blue\">" + item.title + "</a></li>";
                        }).ToArray()
                    ) + "</ul>";
            }

            //show log in or log out link
            if(User.userId > 0)
            {
                scaffold["logout"] = "1";
            }
            else
            {
                scaffold["login"] = "1";
            }

            //include dashboard resources
            //AddScript("/js/views/dashboard/dashboard.js");
            AddCSS("/css/views/dashboard/dashboard.css");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }

        public void AddMenuItem(string id, string title, string url)
        {
            menuItems.Add(new menuItem() { id = id, title = title, url = url });
        }
    }
}
