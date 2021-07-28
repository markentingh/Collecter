using System.Collections.Generic;
using System.Linq;

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

        public override string Render(string body = "")
        {
            //check security first
            if (CheckSecurity() == false) { return AccessDenied(); }

            //setup viewing variables
            View view = new View("/Views/Dashboard/dashboard.html");

            //set title
            view["title"] = "Collector";

            //load body
            view["content"] = body;

            //load custom menu
            if(menuItems.Count > 0)
            {
                view["menu"] = "<ul class=\"tabs right\">" +
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
            if(User.UserId > 0)
            {
                view["logout"] = "1";
            }
            else
            {
                view["login"] = "1";
            }

            //include dashboard resources
            //AddScript("/js/views/dashboard/dashboard.js");
            AddCSS("/css/views/dashboard/dashboard.css");

            //finally, render page
            return base.Render(view.Render());
        }

        public void AddMenuItem(string id, string title, string url)
        {
            menuItems.Add(new menuItem() { id = id, title = title, url = url });
        }
    }
}
