using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Collector.Common.Platform;
using Collector.Common.Analyze;
using Collector.Models.Article;
using Utility.DOM;
using Utility.Strings;

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
                var relpath = Article.ContentPath(url);
                var filepath = Server.MapPath(relpath);
                var filename = articleInfo.articleId + ".html";
                
                if (File.Exists(filepath + filename))
                {
                    //open cached content from disk
                    article = Html.DeserializeArticle(File.ReadAllText(filepath + filename));
                    await Clients.Caller.SendAsync("update", 2, "Loaded cached content for URL: <a href=\"" + url + "\" target=\"_blank\">" + url + "</a>");
                    download = false;
                    Article.FileSize(article);
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
                    var obj = Article.Download(url);
                    article = Html.DeserializeArticle(obj);

                    //get filesize of article
                    Article.FileSize(article);

                    if (article.rawHtml.Length == 0)
                    {
                        //article HTML is empty
                        await Clients.Caller.SendAsync("update", 1, "URL returned an empty response");
                        return;
                    }

                    File.WriteAllText(filepath + filename, obj);
                    await Clients.Caller.SendAsync("update", 1, "Downloaded URL (" + article.fileSize + " KB" + "): <a href=\"" + article.url + "\" target=\"_blank\">" + article.url + "</a>");
                }

                //set article information
                article.url = url;
                article.id = articleInfo.articleId;
                article.feedId = articleInfo.feedId ?? -1;
                article.domain = Web.GetDomainName(url);

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

                Html.GetContentFromDOM(article, tagNames, textElements, anchorElements, headerElements, imgElements, parentIndexes);
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

                Html.GetWordsFromDOM(article, textElements);
                Html.GetArticleElements(article);

                var imgCount = 0;
                var imgTotalSize = 0;

                if(article.body.Count > 0)
                {
                    //found article content
                    await Clients.Caller.SendAsync("update", 1, "Found article text...");
                    Html.GetImages(article);
                    
                    if(article.images.Count > 0)
                    {
                        //images exist, download related images for article
                        await Clients.Caller.SendAsync("update", 1, "Downloading images for article...");

                        //build image path within wwwroot folder
                        var imgpath = "/wwwroot/" + relpath.ToLower() + article.id + "/";

                        //check if img folder exists
                        if (!Directory.Exists(Server.MapPath(imgpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(imgpath));
                        }

                        var cachedCount = 0;

                        for(var x = 0; x < article.images.Count; x++)
                        {
                            //download each image
                            var img = article.images[x];
                            var path = Server.MapPath(imgpath + img.index + "." + img.extension);
                            if (!File.Exists(path))
                            {
                                try
                                {
                                    using (WebClient webClient = new WebClient())
                                    {
                                        webClient.DownloadFile(new Uri(img.url), path);
                                        var filesize = File.ReadAllBytes(path).Length / 1024;
                                        imgCount++;
                                        imgTotalSize += filesize;
                                        await Clients.Caller.SendAsync("update", 1, "Downloaded image \"" + img.filename + "\" (" + filesize + " kb)");
                                    }
                                }catch(Exception)
                                {
                                    await Clients.Caller.SendAsync("update", 1, "Image Download Error: \"" + img.filename + "\"");
                                }
                            }
                            else
                            {
                                cachedCount++;
                                imgCount++;
                            }
                            if (File.Exists(path))
                            {
                                var filesize = File.ReadAllBytes(path).Length / 1024;
                                imgTotalSize += filesize;
                                article.images[x].exists = true;
                            }
                        }

                        if(cachedCount > 0)
                        {
                            await Clients.Caller.SendAsync("update", 1, cachedCount + " images have already been cached on the server");
                        }
                    }

                    //render article
                    html = Components.Accordion.Render("Article Text", "article-text", Article.RenderArticle(article), false);
                    await Clients.Caller.SendAsync("append", html);
                }

                //update article info in database
                await Clients.Caller.SendAsync("update", 1, "Updating database records...");

                articleInfo.title = article.pageTitle;
                articleInfo.analyzecount++;
                articleInfo.analyzed = Article.Version;
                articleInfo.cached = true;
                articleInfo.domain = article.domain;
                articleInfo.feedId = article.feedId;
                articleInfo.fiction = (short)(article.fiction == true ? 1 : 0);
                articleInfo.filesize = article.fileSize + imgTotalSize;
                articleInfo.images = Convert.ToByte(imgCount);
                articleInfo.importance = (short)article.importance;
                articleInfo.importantcount = (short)article.totalImportantWords;
                articleInfo.paragraphcount = (short)article.totalParagraphs;
                articleInfo.relavance = (short)article.relevance;
                try
                {
                    var subj = article.subjects.OrderBy(a => a.score * -1).First();
                    if(subj != null)
                    {
                        articleInfo.score = (short)subj.score;
                        articleInfo.subjectId = subj.id;
                        articleInfo.subjects = Convert.ToByte(article.subjects.Count);
                    }
                }
                catch (Exception) { }
                articleInfo.sentencecount = (short)article.totalSentences;
                articleInfo.summary = article.summary;
                articleInfo.wordcount = article.totalWords;
                articleInfo.yearstart = (short)article.yearStart;
                articleInfo.yearend = (short)article.yearEnd;
                try
                {
                    articleInfo.years = string.Join(",", article.years.ToArray());
                }
                catch (Exception) { }
                Query.Articles.Update(articleInfo);

                //finished
                await Clients.Caller.SendAsync("update", 1, "Done!");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("update", 1, "Error: " + ex.Message + "<br/>" + ex.StackTrace.Replace("\n", "<br/>"));
            }
        }
    }
}
