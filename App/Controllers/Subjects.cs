using Microsoft.AspNetCore.Http;

namespace Collector.Controllers
{
    public class Subjects : Partials.Dashboard
    {
        public Subjects(HttpContext context, Parameters parameters) : base(context, parameters) { }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddsubjects", "Add Subjects", "");

            //load subjects scaffold HTML
            var scaffold = new Scaffold("/Views/Subjects/subjects.html");

            //load subjects list
            try
            {
                var inject = Common.Platform.Subjects.RenderList();
                scaffold["content"] = inject.html;
                if(inject.javascript != "")
                {
                    scripts.Append("<script language=\"javascript\">" + inject.javascript + "</script>");
                }
            }catch(ServiceErrorException)
            {
                scaffold["content"] = "";
                scaffold["no-subjects"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/subjects/subjects.css");
            AddScript("/js/views/subjects/subjects.js");
            
            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
