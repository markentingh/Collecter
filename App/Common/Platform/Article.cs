using System;
using System.Collections.Generic;
using System.Linq;
using Utility.Strings;

namespace Collector.Common.Platform
{
    public static class Article
    {
        public static string ContentPath(string url)
        {
            //get content path for url
            var domain = url.GetDomainName();
            return "/Content/articles/" + domain.Substring(0, 2) + "/" + domain + "/";
        }

        public static Models.Article.AnalyzedArticle Download(string url)
        {
            var path = Server.MapPath(Server.Instance.Cache["browserPath"].ToString());
            var html = Utility.Shell.Execute(path, "-url " + url, path.Replace("WebBrowser.exe",""));
            var article = new Models.Article.AnalyzedArticle(url, html);
            return article;
        }

        public static Query.Models.Article Add(string url)
        {
            var ver = Server.Instance.Version.Split('.');
            var version = double.Parse(ver[0] + "." + string.Join("", ver.Skip(1)));
            var article = new Query.Models.Article()
            {
                active = true,
                analyzecount = 0,
                analyzed = version,
                cached = false,
                datecreated = DateTime.Now,
                datepublished = DateTime.Now,
                deleted = false,
                domain = url.GetDomainName(),
                feedId = 0,
                fiction = 0,
                filesize = 0,
                images = 0,
                importance = 0,
                importantcount = 0,
                paragraphcount = 0,
                relavance = 0,
                score = 0,
                sentencecount = 0,
                subjectId = 0,
                subjects = 0,
                summary = "",
                title = url.Replace("http://", "").Replace("https://", "").Replace("www.", ""),
                url = url,
                wordcount = 0,
                yearend = 0,
                years = "",
                yearstart = 0
            };
            article.articleId = Query.Articles.Add(article);
            return article;
        }
    }
}
