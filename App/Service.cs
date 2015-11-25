using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using System.IO;
using Newtonsoft.Json;

namespace Collector
{

    public enum enumInjectTypes
    {
        replace = 0,
        append = 1,
        before = 2,
        after = 3
    }

    public class PageRequest
    {
        public string url = "";
        public string pageTitle = "";
        public string js = "";
        public string css = "";
    }

    public class Inject
    {
        public string element = "";
        public string html = "";
        public string js = "";
        public string css = "";
        public string cssid = "";
        public enumInjectTypes inject = 0;
    }

    public class WebRequest
    {
        public string html = "";
        public string contentType = "text/html";
    }

    public class Service
    {
        protected Core S;
        protected string[] Paths;
        public Dictionary<string, string> Form = new Dictionary<string, string>();
        public IFormFileCollection Files;

        public Service(Core CollectorCore, string[] paths) {
            S = CollectorCore;
            Paths = paths;
        }

        public struct structResponse
        {
            public string html;
            public string css;
            public string js;
            public string window;
        }

        protected structResponse lostResponse()
        {
            //if session is lost, reload the page
            structResponse response = new structResponse();
            response.js = "S.lostSession();";
            return response;
        }

        protected Inject lostInject()
        {
            //if session is lost, reload the page
            Inject response = new Inject();
            response.js = "S.lostSession();";
            return response;
        }

        protected PageRequest lostPageRequest()
        {
            //if session is lost, reload the page
            PageRequest response = new PageRequest();
            response.js = "S.lostSession();";
            return response;
        }

        protected string CompileJs()
        {
            if (S.Page.postJScode != null)
            {
                S.Page.postJS += string.Join("\n", S.Page.postJScode) + S.Page.postJSLast;
            }
            return S.Page.postJS;
        }

        protected string CompileCss()
        {
            if (S.Page.postCSS != null)
            {
                return string.Join("\n", S.Page.postCSS);
            }
            return "";
        }

        protected void SaveEnable()
        {
            //forces client-side to add save point
            S.Page.RegisterJS("savepoint", "S.editor.save.add('','','');");
        }

        protected string RenderHelpColumn(string filename)
        {
            if(S.Server.Cache.ContainsKey(filename) == false || S.isLocal == true)
            {
                string htm = "<div class=\"column-help dashboard-only\"><div>" + File.ReadAllText(S.Server.MapPath(filename)) + "</div></div>";
                S.Server.Cache[filename] = htm;
                return htm;
            }else
            {
                return (string)S.Server.Cache[filename];
            }

        }
    }
}
