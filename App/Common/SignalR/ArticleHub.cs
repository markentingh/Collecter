using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Collector.Common.Platform;
using Collector.Common.Analyze;
using Collector.Models.Article;
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
                    await Clients.Caller.SendAsync("update", 1, "Loaded cached content for URL: <a href=\"" + url + "\" target=\"_blank\">" + url + "</a>");
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
                    var result = Article.Download(url);
                    if(result == "")
                    {
                        await Clients.Caller.SendAsync("update", 1, "Download timed out for URL: <a href=\"" + url + "\" target=\"_blank\">" + url + "</a>");
                        return;
                    }
                    try
                    {
                        article = Html.DeserializeArticle(result);
                    }
                    catch (Exception)
                    {
                        await Clients.Caller.SendAsync("update", 1, "Error parsing DOM!");
                        await Clients.Caller.SendAsync("update", 1, result.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\n","<br/>"));
                        return;
                    }


                    //get filesize of article
                    Article.FileSize(article);

                    if (article.rawHtml.Length == 0)
                    {
                        //article HTML is empty
                        await Clients.Caller.SendAsync("update", 1, "URL returned an empty response");
                        return;
                    }

                    File.WriteAllText(filepath + filename, result);
                    await Clients.Caller.SendAsync("update", 1, "Downloaded URL (" + article.fileSize + " KB" + "): <a href=\"" + url + "\" target=\"_blank\">" + url + "</a>");
                }

                //set article information
                article.url = url;
                article.id = articleInfo.articleId;
                article.feedId = articleInfo.feedId ?? -1;
                article.domain = Web.GetDomainName(url);
                Html.GetArticleInfoFromDOM(article);
                
                await Clients.Caller.SendAsync("update", 1, "Parsed DOM tree (" + article.elements.Count + " elements)");

                //Html.GetWordsFromDOM(article, textElements);
                await Clients.Caller.SendAsync("update", 1, "Analyzing DOM...");
                var bestIndexes = new List<AnalyzedElement>();
                var badIndexes = new List<AnalyzedElement>();
                Html.GetBestElementIndexes(article, bestIndexes, badIndexes);
                Html.GetArticleElements(article, bestIndexes, badIndexes);
                await Clients.Caller.SendAsync("update", 1, "Collected article contents from DOM");


                //send accordion with raw HTML to client
                var rawhtml = Article.RenderRawHTML(article, bestIndexes, badIndexes);
                var html = Components.Accordion.Render("Raw HTML", "raw-html", "<div class=\"empty-top\"></div><div class=\"empty-bottom\"></div>", false);
                await Clients.Caller.SendAsync("append", html);
                await Clients.Caller.SendAsync("rawhtml", rawhtml);
                await Clients.Caller.SendAsync("update", 1, "Generated Raw HTML for dissecting DOM importance");


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

                articleInfo.title = article.title;
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
