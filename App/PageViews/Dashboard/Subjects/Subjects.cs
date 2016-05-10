namespace Collector.PageViews
{
    public class Subjects : PageView
    {
        public Subjects(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup dashboard menu
            string menu = 
                "<div class=\"btn-addsubjects left\"><ul>" + 
                    "<li><a href=\"javascript:\" id=\"btnaddsubjects\" class=\"button blue\">Add Subjects</a></li>" +
                "</ul></div>";
            parentScaffold.Data["menu"] = menu;

            if (scaffold == null)
            {
                //get subjects list from web service
                scaffold = new Scaffold(S, "/app/pageviews/dashboard/subjects/list.html", "", new string[] { "content" });
                Services.Subjects subjects = new Services.Subjects(S, S.Page.Url.paths);
                scaffold.Data["content"] = subjects.LoadSubjectsUI(0);
                S.Page.RegisterJSFromFile("/app/pageviews/dashboard/subjects/list.js");
            }
            return scaffold.Render();
        }
    }
}