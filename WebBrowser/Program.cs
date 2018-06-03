using System;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace WebBrowser
{
    class Program
    {
        private static ChromiumWebBrowser browser;
        private static string url = "http://markentingh.io";
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
            browser = new ChromiumWebBrowser();

            //Browser Initialized Event
            browser.BrowserInitialized += delegate
            {
                browser.Load(url);
            };

            //Frame Load End Event
            browser.FrameLoadEnd += delegate
            {
                Task task = Task.Run(() => {
                    object js = EvaluateScript("document.getElementsByTagName('html')[0].innerHTML;");
                    html = js.ToString();
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
    }
}
