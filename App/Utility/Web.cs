using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace Collector.Utility
{
    public class Web
    {

        public struct structDownloadInfo
        {
            public string url;
            public string html;
        }

        private Core S;

        public Web(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public string Download(string url, bool usePhantomJs = false)
        {
            if (usePhantomJs == true)
            {
                var htm = "";
                var file = S.Server.MapPath("/phantomjs/file.html");
                if (File.Exists(file)){
                    //delete existing file.html
                    File.Delete(file);
                }
                
                S.Util.Shell.Execute("cmd.exe", "/k \"phantomjs" +
                " --output-encoding=utf8 --ignore-ssl-errors=true --local-to-remote-url-access=true" +
                " render.js " + url +
                "\"", S.Server.MapPath("PhantomJs"), 0);

                //check if file.html exists
                var i = 0;
                while(File.Exists(file) == false)
                {
                    if(i >= 30) { break; } //timeout after 10 seconds
                    System.Threading.Thread.Sleep(1000);
                    i++;
                }
                if (File.Exists(file))
                {
                    i = 0;
ReadFile:
                    try
                    {
                        htm = File.ReadAllText(file);
                    }catch(Exception ex)
                    {
                        System.Threading.Thread.Sleep(250);
                        i++;
                        if(i < 10) { goto ReadFile; }
                    }
                    System.Threading.Thread.Sleep(250);
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex) { }
                    
                }
                
                return htm;
            }
            else
            {
                try
                {
                    using (var http = new HttpClient())
                    {
                        return Task.Run(() => http.GetStringAsync(url)).Result;
                    }
                }
                catch (Exception ex)
                {
                    return "";
                }

            }

        }

        public structDownloadInfo DownloadFromPhantomJS(string url)
        {
            var d = new structDownloadInfo();
            d.html = Download(url, true);
            d.url = url;
            var str = d.html.Split(new string[] { "{\\!/}" }, 2, StringSplitOptions.None);
            if (str.Length == 2)
            {
                d.url = str[0];
                d.html = str[1];
            }
            return d;
        }
    }
}
