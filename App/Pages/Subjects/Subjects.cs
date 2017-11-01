namespace Collector.Pages
{
    public class Subjects : Page
    {
        public Subjects(Core CollectorCore) : base(CollectorCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load dashboard interface
            var dashboard = new Partials.Dashboard(S);

            //add custom menu to dashboard
            dashboard.AddMenuItem("btnaddsubjects", "Add Subjects", "");

            //load subjects scaffold HTML
            var scaffold = new Scaffold(S, "/Pages/Subjects/subjects.html");

            //load subjects list
            var subjects = new Services.Subjects(S);
            var inject = subjects.InjectSubjectsUI();
            scaffold.Data["content"] = inject.html;
            if(scaffold.Data["content"] == Error())
            {
                scaffold.Data["content"] = "";
                scaffold.Data["no-subjects"] = "1";
            }

            //add page resources
            dashboard.AddScript("/js/pages/subjects/subjects.js");
            dashboard.AddCSS("/css/pages/subjects/subjects.css");

            //finally, render page
            return dashboard.Render(path, scaffold.Render(), metadata);
        }
    }
}
