namespace Collector.Controllers
{
    public class Subjects : Partials.Dashboard
    {
        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //add custom menu to dashboard
            AddMenuItem("btnaddsubjects", "Add Subjects", "");

            //load subjects view HTML
            var view = new View("/Views/Subjects/subjects.html");

            //load subjects list
            try
            {
                var inject = Common.Platform.Subjects.RenderList();
                view["content"] = inject.html;
                if(inject.javascript != "")
                {
                    Scripts.Append("<script language=\"javascript\">" + inject.javascript + "</script>");
                }
            }catch(LogicException)
            {
                view["content"] = "";
                view["no-subjects"] = "1";
            }

            //add CSS & JS files
            AddCSS("/css/views/subjects/subjects.css");
            AddScript("/js/views/subjects/subjects.js");
            
            //finally, render page
            return base.Render(view.Render());
        }
    }
}
