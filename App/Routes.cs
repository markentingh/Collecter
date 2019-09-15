using Microsoft.AspNetCore.Http;

public class Routes : Datasilk.Web.Routes
{
    public override Datasilk.Mvc.Controller FromControllerRoutes(HttpContext context, Parameters parameters, string name)
    {

        switch (name)
        {
            case "login": case "": return new Collector.Controllers.Login(context, parameters);
            case "subjects": return new Collector.Controllers.Subjects(context, parameters);
            case "articles": return new Collector.Controllers.Articles(context, parameters);
            case "article": return new Collector.Controllers.Article(context, parameters);
            default: return null;
        }
    }

    public override Datasilk.Web.Service FromServiceRoutes(HttpContext context, Parameters parameters, string name)
    {
        return null;
    }
}
