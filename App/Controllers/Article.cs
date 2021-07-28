namespace Collector.Controllers
{
    public class Article : Partials.Dashboard
    {

        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load articles view HTML
            var view = new View("/Views/Article/article.html");

            view["content"] = Components.Accordion.Render(
                "Analyze Article: " + Context.Request.Query["url"],
                "analyze-article", 
                Cache.LoadFile("/Views/Article/analyze.html")
            );

            //add CSS & JS files
            AddCSS("/css/views/article/article.css");
            AddScript("/js/utility/signalr/signalr.js");
            AddScript("/js/views/article/article.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
