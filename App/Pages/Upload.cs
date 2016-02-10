using System;

namespace Collector.Pages
{
    public class Upload : Page
    {
        public Upload() : base()
        {
        }

        public override string Render()
        {
            //check security first
            if (CheckSecurity() == false) { return RenderAccessDenied(); }

            //setup scaffolding variables
            Scaffold scaffold = new Scaffold(S, "/app/pages/upload.html", "", new string[] { "type" });
            scaffold.Data["type"] = S.Request.Query["type"];
            return scaffold.Render();
        }
    }
}
