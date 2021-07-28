using System.Collections.Generic;
using System.Text;
using Datasilk.Core.Web;

namespace Collector
{

    public abstract class Controller : Request, IRequest, IController
    {
        public StringBuilder Scripts { get; set; } = new StringBuilder();
        public StringBuilder Css { get; set; } = new StringBuilder();
        private List<string> Resources = new List<string>();
        public bool UsePlatform { get; set; } = true;
        public string Title { get; set; } = "Collector";
        public string Description { get; set; } = "";
        public string Favicon { get; set; } = "/images/favicon.png";
        public string Theme { get; set; } = "default";
        public StringBuilder Footer { get; set; }

        protected IUser user;
        public IUser User
        {
            get
            {
                if (user == null)
                {
                    user = Collector.User.Get(Context);
                }
                return user;
            }
            set { user = value; }
        }

        public virtual void Init() { }

        public virtual string Render(string body = "") {

            if (App.Environment == Environment.development) { ViewCache.Clear(); }
            Scripts.Append("<script language=\"javascript\">S.svg.load('/themes/default/icons.svg?v=" + Server.Version + "');</script>");
            var view = new View("/Views/Shared/layout.html");
            view["title"] = Title;
            view["description"] = Description;
            view["theme"] = Theme;
            view["head-css"] = Css.ToString();
            view["favicon"] = Favicon;
            view["body"] = body;

            //add initialization script
            view["scripts"] = Scripts.ToString();

            return view.Render();
        
        }

        public override void Dispose()
        {
            base.Dispose();
            if(user != null)
            {
                User.Save();
            }
        }

        public virtual bool CheckSecurity(string key = "")
        {
            return true;
        }

        public string AccessDenied<T>() where T : Datasilk.Core.Web.IController
        {
            return Datasilk.Core.Web.IController.AccessDenied<T>(this);
        }

        public virtual string AccessDenied() {
            throw new System.NotImplementedException();
        }

        public string Error<T>() where T : Datasilk.Core.Web.IController
        {
            Context.Response.StatusCode = 500;
            return Datasilk.Core.Web.IController.Error<T>(this);
        }

        public string Error(string message = "Error 500")
        {
            Context.Response.StatusCode = 500;
            return message;
        }

        public string Error404<T>() where T : Datasilk.Core.Web.IController
        {
            Context.Response.StatusCode = 404;
            return Datasilk.Core.Web.IController.Error404<T>(this);
        }

        public string Error404(string message = "Error 404")
        {
            Context.Response.StatusCode = 404;
            return message;
        }

        public string Redirect(string url)
        {
            return "<script language=\"javascript\">window.location.href = '" + url + "';</script>";
        }

        public void AddScript(string url, string id = "", string callback = "")
        {
            if (ContainsResource(url)) { return; }
            Scripts.Append("<script language=\"javascript\"" + (id != "" ? " id=\"" + id + "\"" : "") + " src=\"" + url + "\"" +
                (callback != "" ? " onload=\"" + callback + "\"" : "") + "></script>");
        }

        public void AddCSS(string url, string id = "")
        {
            if (ContainsResource(url)) { return; }
            Css.Append("<link rel=\"stylesheet\" type=\"text/css\"" + (id != "" ? " id=\"" + id + "\"" : "") + " href=\"" + url + "\"></link>");
        }

        public bool ContainsResource(string url)
        {
            if (Resources.Contains(url)) { return true; }
            Resources.Add(url);
            return false;
        }
    }
}