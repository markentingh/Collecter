using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;

namespace WebBrowser
{
    class Program
    {
        private static ChromiumWebBrowser browser;
        private static string url = "http://www.adriancourreges.com/blog/2016/09/09/doom-2016-graphics-study/";
        private static string html = "";

        static void Main(string[] args)
        {
            //parse arguments
            for(var x = 0; x < args.Length; x++)
            {
                switch (args[x])
                {
                    case "-url":
                        if(x - 2 < args.Length)
                        {
                            url = args[x + 1];
                        }
                        break;
                }
            }

            //Create Browser Instance
            var settings = new BrowserSettings()
            {
                ImageLoading = CefState.Disabled,
                Plugins = CefState.Disabled,
                WebGl = CefState.Disabled,
                WindowlessFrameRate = 5
            };
            browser = new ChromiumWebBrowser(url, settings);

            //Frame Load End Event
            browser.FrameLoadEnd += delegate
            {
                Task task = Task.Run(() => {
                    //object js = EvaluateScript("document.getElementsByTagName('html')[0].outerHTML;");
                    var js = File.ReadAllText(Path + "extractDOM.js");
                    object result = EvaluateScript(js);
                    html = JsonConvert.SerializeObject(result, Formatting.None);
                    //html = result.ToString();
                });
            };

            var i = 0;
            while(i++ < 60)
            {
                if(html != "") {
                    Console.Write(html); break;
                }
                Thread.Sleep(1000);
            }

            //Dispose Browser
            Cef.Shutdown();
        }

        private static object EvaluateScript(string script)
        {
            var task = browser.EvaluateScriptAsync(script);
            task.Wait();
            var response = task.Result;
            return response.Success ? (response.Result ?? "") : response.Message;
        }

        private static string Path
        {
            get { return Assembly.GetExecutingAssembly().Location.Replace("WebBrowser.exe", "");  }
        }
    }
}
