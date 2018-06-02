using Microsoft.AspNetCore.Http;
using Datasilk;

public class Routes : Datasilk.Routes
{
    public override Page FromPageRoutes(HttpContext context, string name)
    {

        switch (name)
        {
            case "login": return new Collector.Pages.Login(context);
            case "subjects": return new Collector.Pages.Subjects(context);
            case "articles": return new Collector.Pages.Articles(context);
            case "article": return new Collector.Pages.Article(context);
            default: return null;
        }
    }

    public override Service FromServiceRoutes(HttpContext context, string name)
    {
        return null;
    }
}
