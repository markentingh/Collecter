using Microsoft.AspNetCore.Http;

namespace Collector.Controllers
{
    public class Articles : Partials.Dashboard
    {
        public Articles(HttpContext context, Parameters parameters) : base(context, parameters) { }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddarticle", "Add Article", "");

            //load articles scaffold HTML
            var scaffold = new Scaffold("/Views/Articles/articles.html");

            //load articles list
            try
            {
                scaffold["expanded"] = "expanded";
                scaffold["content"] = Common.Platform.Articles.RenderList();
            }
            catch (ServiceErrorException)
            {
                scaffold["content"] = "";
                scaffold["no-articles"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/articles/articles.css");
            AddScript("/js/views/articles/articles.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
