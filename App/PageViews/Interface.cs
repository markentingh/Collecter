
namespace Collector.PageViews
{
    public class Interface : PageView
    {
        public Interface(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {
        }

        public override string Render()
        {
            return Render("");
        }

        public string Render(string content = "", string cssFile = "", string scriptFiles = "", string headerMenu = "")
        {
            Scaffold scaffold;

            //setup scaffolding variables
            scaffold = new Scaffold(S, "/app/pageviews/interface.html", "", new string[] { });
            scaffold.Data["title"] = S.Server.config.GetSection("website:title").Value;
            scaffold.Data["content"] = content;
            scaffold.Data["cssfile"] = cssFile;
            scaffold.Data["scriptfiles"] = scriptFiles;
            scaffold.Data["script"] = S.Page.RenderJS();
            scaffold.Data["header-menu"] = headerMenu;

            if(S.User.userId == 0)
            {
                scaffold.Data["signin"] = "1";
            }
            else
            {
                scaffold.Data["signout"] = "1";
            }

            return scaffold.Render();
        }
    }
}
