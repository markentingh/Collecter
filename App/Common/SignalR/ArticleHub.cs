using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Collector.Common.Platform;
using Collector.Models.Article;
using Utility.DOM;

namespace Collector.SignalR.Hubs
{
    public class ArticleHub : Hub
    {
        public async Task AnalyzeArticle(string url)
        {
            await Clients.Caller.SendAsync("update", 1, "Collector v" + Server.Instance.Version);

            // Get Article HTML Content //////////////////////////////////////////////////////////////////////////////////////////////////
            var download = true;
            AnalyzedArticle article = new AnalyzedArticle();
            var articleInfo = Query.Articles.GetByUrl(url);

            if (articleInfo != null)
            {
                //article exists in database
                await Clients.Caller.SendAsync("update", 1, "Article exists in database");

                if (File.Exists(Server.MapPath(Article.ContentPath(url) + articleInfo.articleId + ".html"))){
                    //open cached content from disk
                    await Clients.Caller.SendAsync("update", 2, "Loaded cached content for URL: " + url);
                    download = false;
                }
            }

            if(download == true)
            {
                //download article from the internet
                await Clients.Caller.SendAsync("update", 1, "Downloading...");
                article = Article.Download(url);
                await Clients.Caller.SendAsync("update", 1, "Downloaded URL (" + (Encoding.Unicode.GetByteCount(article.rawHtml) / 1024).ToString("c").Replace("$", "").Replace(".00", "") + " KB" + "): " + article.url);
            }

            if(article.rawHtml.Length == 0)
            {
                //article HTML is empty
                await Clients.Caller.SendAsync("update", 1, "URL returned an empty response");
                return;
            }

            // Parse DOM Tree ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            article.elements = (new Parser(article.rawHtml)).Elements;
            article.rawHtml = Common.Analyze.Html.FormatHtml(article.elements).ToString();
            await Clients.Caller.SendAsync("update", 1, "Parsed DOM tree (" + article.elements.Count + " elements)");

            // Collect Content from DOM Tree /////////////////////////////////////////////////////////////////////////////////////////////
            var tagNames = new List<AnalyzedTag>();
            var parentIndexes = new List<AnalyzedParentIndex>();

            //sort elements into different lists
            var textElements = new List<DomElement>();
            var anchorElements = new List<DomElement>();
            var headerElements = new List<DomElement>();
            var imgElements = new List<DomElement>();

            Common.Analyze.Html.GetContent(ref article, ref tagNames, ref textElements, ref anchorElements, ref headerElements, ref imgElements, ref parentIndexes);
            await Clients.Caller.SendAsync("update", 1, "Collected content from DOM tree (" + textElements.Count + " text elements, " + headerElements.Count + " header elements)");

            // Sort Content /////////////////////////////////////////////////////////////////////////////////////////////
            textElements = textElements.OrderBy(p => p.text.Length * -1).ToList();
            headerElements = headerElements.OrderBy(p => p.tagName).ToList();
            parentIndexes = parentIndexes.OrderBy(p => (p.elements.Count * p.textLength) * -1).ToList();
            article.tagNames = tagNames.OrderBy(t => t.count * -1).ToList();
        }
    }
}
