using Microsoft.AspNetCore.Http;

namespace Collector.Pages
{
    public class Articles : Partials.Dashboard
    {
        public Articles(HttpContext context) : base(context) {}

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddarticle", "Add Article", "");

            //load articles scaffold HTML
            var scaffold = new Scaffold("/Views/Articles/articles.html", Server.Scaffold);

            //load articles list
            try
            {
                scaffold.Data["content"] = Common.Platform.Articles.RenderList(); ;
            }
            catch (ServiceErrorException)
            {
                scaffold.Data["content"] = "";
                scaffold.Data["no-articles"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/articles/articles.css");
            AddScript("/js/views/articles/articles.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
