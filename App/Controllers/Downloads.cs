namespace Collector.Controllers
{
    public class Downloads : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load downloads view HTML
            var view = new View("/Views/Downloads/downloads.html");


            //add CSS & JS files
            AddCSS("/css/views/search/downloads.css");
            AddScript("/js/views/search/downloads.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
