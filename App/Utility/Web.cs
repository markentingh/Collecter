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

        public string Download(string url)
        {
            using (var http = new HttpClient())
            {
                return Task.Run(()=> http.GetStringAsync(url)).Result;
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
