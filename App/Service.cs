using Microsoft.AspNetCore.Http;

namespace Collector
{
    public class Service : Datasilk.Web.Service
    {
        public Service(HttpContext context, Parameters parameters) : base(context, parameters) { }
    }
}