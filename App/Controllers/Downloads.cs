namespace Collector.Controllers
{
    public class Downloads : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load articles view HTML
            var view = new View("/Views/Downloads/downloads.html");

            view["content"] = Components.Accordion.Render("Downloads", "", Cache.LoadFile("/Views/Downloads/console.html")
            );

            //add CSS & JS files
            AddCSS("/css/views/downloads/downloads.css");
            AddScript("/js/utility/signalr/signalr.js");
            AddScript("/js/views/downloads/downloads.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
