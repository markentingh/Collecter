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
            default: return new Collector.Pages.Home(context);
        }
    }

    public override Service FromServiceRoutes(HttpContext context, string name)
    {
        return null;
    }
}
