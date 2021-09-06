using System.Text;

namespace Collector.Controllers
{
    public class Feeds : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddfeed", "Add RSS Feed", "");

            //load feeds view HTML
            var view = new View("/Views/Feeds/feeds.html");
            var html = new StringBuilder();
            var categories = Query.Feeds.GetCategories();
            view["category-options"] = Common.Platform.Feeds.RenderOptions(categories);
            view["content"] = Common.Platform.Feeds.RenderList(categories);
            if(view["content"] == "")
            {
                view.Show("no-feeds");
            }

            //add CSS & JS files
            AddCSS("/css/views/feeds/feeds.css");
            AddScript("/js/views/feeds/feeds.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
