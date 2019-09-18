using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Collector.Controllers
{
    public class Downloads : Partials.Dashboard
    {
        public Downloads(HttpContext context, Parameters parameters) : base(context, parameters)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load downloads scaffold HTML
            var scaffold = new Scaffold("/Views/Downloads/downloads.html");


            //add CSS & JS files
            AddCSS("/css/views/search/downloads.css");
            AddScript("/js/views/search/downloads.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
