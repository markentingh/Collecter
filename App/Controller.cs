using Microsoft.AspNetCore.Http;

namespace Collector
{
    public class Controller : Datasilk.Mvc.Controller
    {
        //constructor
        public Controller(HttpContext context, Parameters parameters) : base(context, parameters)
        {
            title = "Collector";
            description = "Collect Knowledge";
        }
    }
}