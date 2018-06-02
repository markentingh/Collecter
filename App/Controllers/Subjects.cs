using Microsoft.AspNetCore.Http;

namespace Collector.Pages
{
    public class Subjects : Partials.Dashboard
    {
        public Subjects(HttpContext context) : base(context) { }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddsubjects", "Add Subjects", "");

            //load subjects scaffold HTML
            var scaffold = new Scaffold("/Views/Subjects/subjects.html", Server.Scaffold);

            //load subjects list
            try
            {
                var inject = Common.Platform.Subjects.RenderList();
                scaffold.Data["content"] = inject.html;
                if(inject.javascript != "")
                {
                    scripts.Append("<script language=\"javascript\">" + inject.javascript + "</script>");
                }
            }catch(ServiceErrorException)
            {
                scaffold.Data["content"] = "";
                scaffold.Data["no-subjects"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/subjects/subjects.css");
            AddScript("/js/views/subjects/subjects.js");
            
            //finally, render page
            return base.Render(path, scaffold.Render(), metadata);
        }
    }
}
