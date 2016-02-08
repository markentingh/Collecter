using System.Net.Http;
using System.Threading.Tasks;

namespace Collector.Utility
{
    public class Web
    {

        private Core S;

        public Web(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public string Download(string url, bool usePhantomJs = false)
        {
            if(usePhantomJs == true)
            {
                var htm = "";

                while (1 == 1)
                {
                    htm = S.Util.Shell.Execute("cmd.exe", "/k \"phantomjs" +
                    " --output-encoding=utf8 --ignore-ssl-errors=true" + // --local-to-remote-url-access=true" +
                    " render.js " + url +
                    "\"", S.Server.MapPath("PhantomJs"), 15);

                    if(htm.Length < 100)
                    {
                        if(htm.IndexOf("fail") >= 0 || htm.IndexOf("cancel") >= 0 || htm == "") { continue; }
                    }
                    break;
                }

                return htm;

            }
            else
            {
                using (var http = new HttpClient())
                {
                    return Task.Run(() => http.GetStringAsync(url)).Result;
                }
            }
            
        }

        public string DownloadFromPhantomJS(string url, int timeout = 3, bool executeJs = true, string postJs = "")
        {
            var html = Download(url).ToString();
            //TODO: Get PhantomJS tool to download webpage for us
            return html;
        }
    }
}
