using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Datasilk.Core.Web;

namespace Collector
{
    public class Service : Request, IRequest, IService
    {
        protected StringBuilder Scripts = new StringBuilder();
        protected StringBuilder Css = new StringBuilder();
        protected List<string> Resources = new List<string>();

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

        public virtual void Init()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            if (user != null)
            {
                User.Save();
            }
        }

        public virtual Service Instantiate(IRequest request)
        {
            Path = request.Path;
            PathParts = request.PathParts;
            Context = request.Context;
            Parameters = request.Parameters;
            Init();
            return this;
        }

        public string JsonResponse(dynamic obj)
        {
            Context.Response.ContentType = "text/json";
            return JsonSerializer.Serialize(obj);
        }

        protected string Response(string html)
        {
            return JsonResponse(new Response(html, Css.ToString() + Scripts.ToString()));
        }

        public string Success()
        {
            return "success";
        }

        public string Empty() { return "{}"; }

        public string AccessDenied(string message = "Error 403")
        {
            Context.Response.StatusCode = 403;
            return message;
        }

        public string Error(string message = "Error 500")
        {
            Context.Response.StatusCode = 500;
            return message;
        }

        public string BadRequest(string message = "Bad Request 400")
        {
            Context.Response.StatusCode = 400;
            return message;
        }

        public void AddScript(string url, string id = "", string callback = "")
        {
            if (ContainsResource(url)) { return; }
            Scripts.Append("S.util.js.load('" + url + "', '" + id + "', " + (callback != "" ? callback : "null") + ");");
        }

        public void AddCSS(string url, string id = "")
        {
            if (ContainsResource(url)) { return; }
            Scripts.Append("S.util.css.load('" + url + "', '" + id + "');");
        }

        protected bool ContainsResource(string url)
        {
            if (Resources.Contains(url)) { return true; }
            Resources.Add(url);
            return false;
        }

        public virtual bool CheckSecurity()
        {
            return User.UserId > 0;
        }

        public static string Inject(string selector, responseType injectType, string html, string javascript, string css)
        {
            var response = new Response()
            {
                type = injectType,
                selector = selector,
                html = html,
                javascript = javascript,
                css = css
            };
            return Inject(response);
        }

        public static string Inject(Response response)
        {
            return JsonSerializer.Serialize(new { d = response });
        }
    }
}