using Microsoft.AspNet.Http;
using System.Linq;

namespace Collector.Pipeline
{
    public class App
    {
        private Core S;
        

        public App(Server server, HttpContext context, string[] paths)
        {
            //the Pipeline.App handles page loading from /App/Pages
            string page = "Home";
            if(paths.Length > 0) {
                if(paths[0] != "")
                {
                    page = paths[0].First().ToString().ToUpper() + paths[0].Substring(1);
                }
            }
            S = new Core(server, context, "", "app", page);

            S.App = this;
            S.isFirstLoad = true;

            //check for web bots such as google bot
            string agent = context.Request.Headers["User-Agent"];
            agent = agent.ToLower();
            if (agent.Contains("bot") | agent.Contains("crawl") | agent.Contains("spider"))
            {
                S.Page.isBot = true;
            }

            //check for mobile agent
            if (agent.Contains("mobile") | agent.Contains("blackberry") | agent.Contains("android") | agent.Contains("symbian") | agent.Contains("windows ce") | 
                agent.Contains("fennec") | agent.Contains("phone") | agent.Contains("iemobile") | agent.Contains("iris") | agent.Contains("midp") | agent.Contains("minimo") | 
                agent.Contains("kindle") | agent.Contains("opera mini") | agent.Contains("opera mobi") | agent.Contains("ericsson") | agent.Contains("iphone") | agent.Contains("ipad"))
            {
                S.Page.isMobile = true;
            }
            if(agent.Contains("tablet") | agent.Contains("ipad")) { S.Page.isTablet = true; }

            //parse URL
            S.Page.GetPageUrl();

            //setup viewstate Id
            S.Page.RegisterJS("viewstate", "S.ajax.viewstateId='" + S.ViewStateId + "';");
            
            //finally, scaffold Collector platform HTML
            S.Response.ContentType = "text/html";
            S.Response.WriteAsync(S.Page.Render());

            //unload the core
            S.Unload();
        }
    }
}
