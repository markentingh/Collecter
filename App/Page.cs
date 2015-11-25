using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Collector
{
    public class Page
    {
        #region "Properties"
        [JsonIgnore]
        protected Core S;

        //Global Variables
        [JsonIgnore]
        public bool isBot = false;
        [JsonIgnore]
        public bool isMobile = false;
        [JsonIgnore]
        public bool isTablet = false;
        
        //Javascript
        [JsonIgnore]
        protected string[] postJSnames = new string[] { }; //used so duplicate JS doesn't get added to the page
        [JsonIgnore]
        public string[] postJScode = new string[] { }; //array of javascript to add
        public string[] postJSonce = new string[] { }; //used so duplicate JS that loads only once doesn't get added to the page
        [JsonIgnore]
        public string postJS = ""; //used to compile javascript for postback response
        [JsonIgnore]
        public string postJSLast = ""; //added to the end of postJS

        //CSS
        [JsonIgnore]
        public string[] postCSS = new string[] { }; //array of CSS to add
        [JsonIgnore]
        protected string[] postCSSnames = new string[] { }; //used so duplicate CSS doesn't get added to the page

        //Request Url Info
        public struct structUrl
        {
            public string path;
            public string host;
            public string query;
            public string[] paths;
        }
        public structUrl Url;

        //Web Services
        [JsonIgnore]
        public PageRequest PageRequest;

        //initialize class
        public Page()
        {
        }

        public void Load(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public virtual string Render()
        {
            return "";
        }
        #endregion

        public void GetPageUrl()
        {
            Url.query = "";
            string path = S.Request.Path.ToString().ToLower().Replace(" ", "+");
            string[] arr = null;
            if(path.Substring(0,1) == "/") { path = path.Substring(1); }
            if(path != "")
            {
                arr = path.Split(new char[] { '/' });
                Url.path = arr[0].Replace("-", " ");
                if(arr.Length > 1)
                {
                    Url.query = path.Split(new char[] { '/' }, 2)[1]; ;
                }
                Url.paths = arr;
            }else
            {
                Url.path = "home";
                Url.paths = new string[]{"home"};
            }

            //get host
            Url.host = S.Request.Host.ToString();
            int start = 0;
            start = Url.host.IndexOf("//");
            if (start >= 0)
            {
                start = Url.host.IndexOf('/', start + 3);
                if (start >= 0)
                {
                    Url.host = Url.host.Substring(0, start);
                }
            }
        }

        #region "Javascript & CSS"
        /// <summary>
        /// <para>Adds your Javascript code to a variable that generates a javascript block at the bottom of the page on Page_Collector, 
        /// either directly on the page, or at the end of an AJAX postback response</para>
        /// <para>No duplicate names are allowed per page or AJAX S.Request.Query, which protects Collector from generating duplicate Javascript code on the page</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="js"></param>
        /// <remarks></remarks>
        public virtual void RegisterJS(string name, string js, bool overwrite = false, bool last = false)
        {
            //register non-duplicated javascript with 
            bool addJs = true;
            //check for duplicate name
            if (postJSnames.Length > 0)
            {
                for (int x = 0; x <= postJSnames.Length - 1; x++)
                {
                    if (postJSnames[x] == name)
                    {
                        if (overwrite == true & last == false)
                        {
                            postJScode[x] = js;
                        }
                        return;
                    }
                }
            }

            Array.Resize(ref postJSnames, postJSnames.Length + 1);
            Array.Resize(ref postJScode, postJScode.Length + 1);

            postJSnames[postJSnames.Length - 1] = name;
            if (last == false)
            {
                postJScode[postJSnames.Length - 1] = js;
            }
            else
            {
                if (addJs == true)
                {
                    postJSLast += js;
                }
            }

        }

        /// <summary>
        /// <para>Adds your Javascript code to a variable that generates a javascript block at the bottom of the page on Page_Render, 
        /// either directly on the page, or at the end of an AJAX postback response.</para>
        /// 
        /// <para>No duplicate names are allowed within the entire page life and view state (page load and all AJAX requests), which 
        /// protects Collector from generating duplicate Javascript code on the page at any given time.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="js"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool RegisterJSonce(string name, string js)
        {
            //register javascript so it only loads once
            //throughout the entire viewstate life

            int i = 0;
            if (postJSonce == null)
            {
                postJSonce = new string[] { };
            }
            else
            {
                for (int x = 0; x <= postJSonce.Length - 1; x++)
                {
                    if (postJSonce[x] == name)
                        return false;
                }
                i = postJSonce.Length;
                Array.Resize(ref postJSonce, i + 1);
            }
            postJSonce[i] = name;
            postJS += js + "\n";
            return true;
        }

        public bool CheckJSOnceIfLoaded(string name)
        {
            if ((postJSonce == null) == false)
            {
                for (int x = 0; x <= postJSonce.Length - 1; x++)
                {
                    if (postJSonce[x] == name)
                        return true;
                }
            }
            return false;
        }

        public void RegisterJSfile(string file, string callback = "")
        {
            string myJs = "$.when(" + "$.getScript('" + file + "')," +
                          "$.Deferred(function(deferred){$(deferred.resolve);})" + ").done(function(){" + callback + "});";
            RegisterJSonce(file, myJs);
        }

        public void RegisterJSFromFile(string name, string file)
        {
            RegisterJS(name, S.Server.OpenFile(file));
        }

        public void RegisterCSS(string name, string css, bool overwrite = false)
        {
            //register non-duplicated css
            if (postCSSnames.Length > 0)
            {
                for (int x = 0; x <= postCSSnames.Length - 1; x++)
                {
                    if (postCSSnames[x] == name)
                    {
                        if (overwrite == true)
                        {
                            postCSS[x] = css;
                        }
                        return;
                    }
                }
            }

            Array.Resize(ref postCSSnames, postCSSnames.Length + 1);
            Array.Resize(ref postCSS, postCSS.Length + 1);

            postCSSnames[postCSSnames.Length - 1] = name;
            postCSS[postCSSnames.Length - 1] = css;
        }

        public void RegisterCSSfile(string file)
        {
            string myJs = "(function(){var f=document.createElement(\"link\");" + "f.setAttribute(\"rel\", \"stylesheet\");" + 
                          "f.setAttribute(\"type\", \"text/css\");" + "f.setAttribute(\"href\", \"" + file + "\");" + 
                          "document.getElementsByTagName(\"head\")[0].appendChild(f);})();";
            RegisterJSonce(file, myJs);
        }

        public string RenderJS()
        {
            //render Javascript
            if (S.Page.postJScode != null)
            {
                S.Page.postJS += string.Join("\n", S.Page.postJScode) + S.Page.postJSLast;
            }
            return S.Page.postJS;
        }
        #endregion

        #region "Security"
        public bool CheckSecurity(string feature = "", enumSecurityType security = enumSecurityType.both)
        {
            return S.User.CheckSecurity(feature, security);
        }

        public string RenderAccessDenied()
        {
            //setup scaffolding variables
            Scaffold scaffold = new Scaffold(S, "/app/pages/signin/form.html", "", new string[] { "error", "errmsg" });
            scaffold.Data["error"] = "1";
            scaffold.Data["errmsg"] = "You do not have access to this page. Please sign in to continue.";

            //load website interface
            Includes.Interface iface = new Includes.Interface(S, scaffold);

            return iface.Render(scaffold.Render(), "signin.css");
        }
        #endregion
    }
}
