using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Collector.Common.Platform;
using Collector.Common.Analyze;
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
            try
            {
                var download = true;
                AnalyzedArticle article = new AnalyzedArticle();
                var articleInfo = Query.Articles.GetByUrl(url);

                if (articleInfo != null)
                {
                    //article exists in database
                    await Clients.Caller.SendAsync("update", 1, "Article exists in database");
                }
                else
                {
                    //create article in database
                    articleInfo = Article.Add(url);
                }
                var filepath = Server.MapPath(Article.ContentPath(url));
                var filename = articleInfo.articleId + ".html";
                
                if (File.Exists(filepath + filename))
                {
                    //open cached content from disk
                    article.rawHtml = File.ReadAllText(filepath + filename);
                    await Clients.Caller.SendAsync("update", 2, "Loaded cached content for URL: " + url);
                    download = false;
                }
                else if (!Directory.Exists(filepath))
                {
                    //create folder for content
                    Directory.CreateDirectory(filepath);
                }

                if (download == true)
                {
                    //download article from the internet
                    await Clients.Caller.SendAsync("update", 1, "Downloading...");
                    article = Article.Download(url);

                    if (article.rawHtml.Length == 0)
                    {
                        //article HTML is empty
                        await Clients.Caller.SendAsync("update", 1, "URL returned an empty response");
                        return;
                    }

                    File.WriteAllText(filepath + filename, article.rawHtml);
                    await Clients.Caller.SendAsync("update", 1, "Downloaded URL (" + (Encoding.Unicode.GetByteCount(article.rawHtml) / 1024).ToString("c").Replace("$", "").Replace(".00", "") + " KB" + "): " + article.url);
                }

                // Parse DOM Tree ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                article.elements = (new Parser(article.rawHtml)).Elements;
                article.rawHtml = Common.Analyze.Html.FormatHtml(article.elements).ToString();

                //send accordion with raw HTML to client
                var html = Components.Accordion.Render("Raw HTML", "raw-html", "<pre>" + article.rawHtml.Replace("&", "&amp;").Replace("<", "&lt;") + "</pre>", false);
                await Clients.Caller.SendAsync("append", html);
                await Clients.Caller.SendAsync("update", 1, "Parsed DOM tree (" + article.elements.Count + " elements)");

                // Collect Content from DOM Tree /////////////////////////////////////////////////////////////////////////////////////////////
                var tagNames = new List<AnalyzedTag>();
                var parentIndexes = new List<AnalyzedParentIndex>();

                //sort elements into different lists
                var textElements = new List<DomElement>();
                var anchorElements = new List<DomElement>();
                var headerElements = new List<DomElement>();
                var imgElements = new List<DomElement>();

                Html.GetContent(article, tagNames, textElements, anchorElements, headerElements, imgElements, parentIndexes);
                await Clients.Caller.SendAsync("update", 1, "Collected content from DOM tree (" + textElements.Count + " text elements, " + headerElements.Count + " header elements)");

                // Sort Content /////////////////////////////////////////////////////////////////////////////////////////////
                textElements = textElements.OrderBy(p => p.text.Length * -1).ToList();
                headerElements = headerElements.OrderBy(p => p.tagName).ToList();
                parentIndexes = parentIndexes.OrderBy(p => (p.elements.Count * p.textLength) * -1).ToList();
                article.tagNames = tagNames.OrderBy(t => t.count * -1).ToList();
                
                foreach (DomElement header in headerElements)
                {
                    article.tags.headers.Add(header.index);
                }
                foreach (DomElement anchor in anchorElements)
                {
                    article.tags.anchorLinks.Add(anchor.index);
                }

                // Analyze Content /////////////////////////////////////////////////////////////////////////////////////////////
                //
                // to determine if there is an article within the HTML page
                // or if the page is simply a link to an article (in which case, follow link to article)
                // or if the page has a paywall in front of the article (in which case abandon the article)

                Html.GetWords(article, textElements);

                Html.GetArticleElements(article);

                if(article.body.Count > 0)
                {
                    //found article
                    html = Components.Accordion.Render("Article Text", "article-text", Article.RenderArticle(article), false);
                    await Clients.Caller.SendAsync("append", html);
                }

            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("update", 1, "Error: " + ex.Message + "<br/>" + ex.StackTrace.Replace("\n", "<br/>"));
            }
        }
    }
}
