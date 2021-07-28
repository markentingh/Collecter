using Microsoft.AspNetCore.Http;
using Datasilk.Core.Web;

namespace Collector
{
    public class Routes : Datasilk.Core.Web.Routes
    {
        public override IController FromControllerRoutes(HttpContext context, Parameters parameters, string name)
        {
            switch (name)
            {
                case "login": case "": return new Controllers.Login();
                case "subjects": return new Controllers.Subjects();
                case "articles": return new Controllers.Articles();
                case "article": return new Controllers.Article();
                default: return null;
            }
        }

        public override IService FromServiceRoutes(HttpContext context, Parameters parameters, string name)
        {
            return null;
        }
    }
}
