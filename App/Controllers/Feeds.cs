namespace Collector.Controllers
{
    public class Feeds : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load feeds view HTML
            var view = new View("/Views/Feeds/feeds.html");


            //add CSS & JS files
            AddCSS("/css/views/feeds/feeds.css");
            AddScript("/js/views/feeds/feeds.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
