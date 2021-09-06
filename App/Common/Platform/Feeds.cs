using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collector.Common.Platform
{
    public static class Feeds
    {
        public static string RenderList(List<Query.Models.FeedCategory> categories)
        {
            var view = new View("/Views/Feeds/list-item.html");
            var feeds = Query.Feeds.GetList();
            var result = new StringBuilder();
            var html = new StringBuilder();
            foreach(var cat in categories)
            {
                html.Clear();
                var catfeeds = feeds.Where(a => a.categoryId == cat.categoryId);
                if(catfeeds.Count() == 0) { continue; }
                foreach (var feed in catfeeds)
                {
                    view.Clear();
                    view["title"] = feed.title;
                    view["last-checked"] = feed.lastChecked.HasValue ? feed.lastChecked.Value.ToString("MM/dd hh:mm tt") : "Never";
                    view["url"] = feed.url;
                    html.Append(view.Render());
                }
                result.Append(Components.Accordion.Render(cat.title, "cat-" + cat.categoryId, html.ToString(), false) + "\n");
            }
            return result.ToString();
        }

        public static string RenderOptions(List<Query.Models.FeedCategory> categories)
        {
            var html = new StringBuilder();
            foreach (var cat in categories)
            {
                html.Append("<option value=\"" + cat.categoryId + "\">" + cat.title + "</option>\n");
            }
            return html.ToString();
        }
    }
}
