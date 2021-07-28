namespace Collector.Controllers
{
    public class Search : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load search view HTML
            var view = new View("/Views/Search/search.html");


            //add CSS & JS files
            AddCSS("/css/views/search/search.css");
            AddScript("/js/views/search/search.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
