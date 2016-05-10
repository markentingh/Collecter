using System;

namespace Collector.Pages
{
    public class Dashboard : Page
    {
        public Dashboard() : base()
        {
        }

        public override string Render()
        {
            //check security first
            if (CheckSecurity() == false) { return RenderAccessDenied(); }

            //setup scaffolding variables
            Scaffold scaffold = new Scaffold(S, "/app/pages/dashboard.html", "", new string[] { "content", "menu", "dev-menu", "admin-menu"});

            //load dashboard section
            var headerMenu = scaffold.Get("dash-menu");
            string sect = "Topics"; //default include to load
            if(Url.paths.Length > 1) { sect = S.Util.Str.Capitalize(Url.paths[1]); }
            string className = "Collector.PageViews." + sect;
            Type classType = Type.GetType(className);
            PageView section = (PageView)Activator.CreateInstance(classType, new object[] { S, scaffold });
            scaffold.Data["content"] = section.Render();

            //load developer-level menu
            if(S.User.userType <= 1)
            {
                scaffold.Data["dev-menu"] = "1";
            }

            //load admin menu
            if (S.User.userType == 0)
            {
                scaffold.Data["admin-menu"] = "1";
            }

            //load website interface
            PageViews.Interface iface = new PageViews.Interface(S, scaffold);

            return iface.Render(scaffold.Render(), "dashboard.css", section.scriptFiles, headerMenu);
        }
    }
}
