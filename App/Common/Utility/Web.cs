using System;
using System.Net;
using System.Threading.Tasks;

namespace Utility.Web
{
    public static class Download
    {
        public static async Task DownloadAsync(string url, string outputFile)
        {
            using (WebClient webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(new Uri(url), outputFile);
            }
        }
    }
}
