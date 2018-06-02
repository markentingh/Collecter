using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Collector.Pages
{
    public class Article : Partials.Dashboard
    {
        public Article(HttpContext context) : base(context) {}

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load articles scaffold HTML
            var scaffold = new Scaffold("/Views/Article/article.html", Server.Scaffold);

            //add CSS & JS files
            AddCSS("/css/views/article/article.css");
            AddScript("/js/views/article/article.js");

            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
