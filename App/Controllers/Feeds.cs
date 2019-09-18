using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Collector.Controllers
{
    public class Feeds : Partials.Dashboard
    {
        public Feeds(HttpContext context, Parameters parameters) : base(context, parameters)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load feeds scaffold HTML
            var scaffold = new Scaffold("/Views/Feeds/feeds.html");


            //add CSS & JS files
            AddCSS("/css/views/feeds/feeds.css");
            AddScript("/js/views/feeds/feeds.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
