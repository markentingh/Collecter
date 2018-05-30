using Microsoft.AspNetCore.Http;

namespace Collector
{
    public class Page : Datasilk.Page
    {
        //constructor
        public Page(HttpContext context) : base(context)
        {
            title = "Collector";
            description = "Collect Knowledge";
        }
    }
}