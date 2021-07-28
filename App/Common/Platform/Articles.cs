using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Collector.Common.Platform
{
    public static class Articles
    {
        public static string RenderList(int subjectId = -1, int feedId = -1, int start = 1, int length = 50, string search = "", Query.Articles.IsActive isActive = Query.Articles.IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, Query.Articles.SortBy orderBy = Query.Articles.SortBy.newest)
        {
            var item = new View("/Views/Articles/list-item.html");
            var html = new StringBuilder();


            List<Query.Models.ArticleDetails> articles;
            var subjectIds = new List<int>();
            if(subjectId > -1)
            {
                subjectIds.Add(subjectId);
            }

            if (feedId >= 0)
            {
                articles = Query.Articles.GetListForFeeds(subjectIds.ToArray(), feedId, search, Query.Articles.IsActive.Both, false, minImages, dateStart, dateEnd, orderBy, start, length);
            }
            else
            {
                articles = Query.Articles.GetList(subjectIds.ToArray(), search, Query.Articles.IsActive.Both, false, minImages, dateStart, dateEnd, orderBy, start, length);
            }

            if(articles != null)
            {
                foreach(var article in articles)
                {
                    //populate view with article info
                    item["title"] = article.title;
                    item["encoded-url"] = WebUtility.UrlEncode(article.url);
                    item["url"] = article.url;

                    if (article.breadcrumb != null && article.breadcrumb.Length > 0)
                    {
                        //show breadcrumb
                        var bread = article.breadcrumb.Split('>');
                        var hier = article.hierarchy.Split('>');
                        var crumb = "";
                        var hasSubject = false;
                        for (var b = 0; b < bread.Length; b++)
                        {
                            crumb += (crumb != "" ? " > " : "") + "<a href=\"/subject/" + hier[b] + "\">" + bread[b] + "</a>";
                            if (int.Parse(hier[b]) == subjectId) { hasSubject = true; }
                        }
                        if (hasSubject == false)
                        {
                            crumb += (crumb != "" ? " > " : "") + "<a href=\"dashboard/subjects?id=" + subjectId + "\">" + article.subjectTitle + "</a>";
                        }
                        item["show-breadcrumb"] = "1";
                        item["breadcrumb"] = crumb;
                        item["score"] = string.Format("{0:N0}", article.score);
                    }
                    else
                    {
                        //hide breadcrumb
                        item["show-breadcrumb"] = "";
                        item["breadcrumb"] = "";
                        item["score"] = "";
                    }
                    
                    if(article.filesize != null && article.filesize > 0)
                    {
                        //show file size
                        item["show-file-size"] = "1";
                        item["file-size"] = Math.Round(article.filesize.Value, 2).ToString();
                    }
                    else
                    {
                        //hide file size
                        item["show-file-size"] = "";
                        item["file-size"] = "";
                    }

                    if (article.wordcount != null && article.wordcount > 0)
                    {
                        //show words
                        item["show-words"] = "1";
                        item["words"] = string.Format("{0:N0}", article.wordcount);
                    }
                    else
                    {
                        //hide words
                        item["show-words"] = "";
                        item["words"] = "";
                    }

                    if (article.sentencecount != null && article.sentencecount > 0)
                    {
                        //show sentences
                        item["show-sentences"] = "1";
                        item["sentences"] = string.Format("{0:N0}", article.sentencecount);
                    }
                    else
                    {
                        //hide sentences
                        item["show-sentences"] = "";
                        item["sentences"] = "";
                    }

                    if (article.importantcount != null && article.importantcount > 0)
                    {
                        //show important words
                        item["show-important-words"] = "1";
                        item["important-words"] = string.Format("{0:N0}", article.importantcount);
                    }
                    else
                    {
                        //hide important words
                        item["show-important-words"] = "";
                        item["important-words"] = "";
                    }

                    if (article.years != null && article.years != "")
                    {
                        //show words
                        item["show-years"] = "1";
                        item["years"] = article.years.Replace(",", ", ");
                    }
                    else
                    {
                        //hide words
                        item["show-years"] = "";
                        item["years"] = "";
                    }

                    html.Append(item.Render());
                }
            }
            return html.ToString();
        }
    }
}
