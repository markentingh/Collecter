using Microsoft.AspNetCore.Http;

namespace Collector
{
    public class Service : Datasilk.Service
    {
        public Service(HttpContext context) : base(context) { }
    }
}