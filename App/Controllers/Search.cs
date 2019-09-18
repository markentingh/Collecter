using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Collector.Controllers
{
    public class Search : Partials.Dashboard
    {
        public Search(HttpContext context, Parameters parameters) : base(context, parameters)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load search scaffold HTML
            var scaffold = new Scaffold("/Views/Search/search.html");


            //add CSS & JS files
            AddCSS("/css/views/search/search.css");
            AddScript("/js/views/search/search.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
