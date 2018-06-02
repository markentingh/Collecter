using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Collector.SignalR.Hubs
{
    public class ArticleHub : Hub
    {
        public async Task AnalyzeArticle(string url)
        {

        }
    }
}
