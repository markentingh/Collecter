namespace Collector.Controllers
{
    public class Articles : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddarticle", "Add Article", "");

            //load articles view HTML
            var view = new View("/Views/Articles/articles.html");

            //load articles list
            try
            {
                view["expanded"] = "expanded";
                view["content"] = Common.Platform.Articles.RenderList();
            }
            catch (LogicException)
            {
                view["content"] = "";
                view["no-articles"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/articles/articles.css");
            AddScript("/js/views/articles/articles.js");

            //finally, render page
            return base.Render(view.Render());
        }
    }
}
