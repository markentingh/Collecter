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
            var path = Server.Instance.Cache["browserPath"].ToString();
            var html = Utility.Shell.Execute(path, Server.MapPath("-url " + url), path.Replace("WebBrowser.exe",""));
            var article = new Models.Article.AnalyzedArticle(url, html);
            return article;
        }

        public static Query.Models.Article Add(string url)
        {
            var ver = Server.Instance.Version.Split('.');
            var version = double.Parse(ver[0] + "." + string.Join("", ver.Skip(1)));
            var article = new Query.Models.Article()
            {
                url = url,
                domain = url.GetDomainName(),
                title = url.Replace("http://", "").Replace("https://", "").Replace("www.", ""),
                analyzed = version
            };
            article.articleId = Query.Articles.Add(article);
            return article;
        }
    }
}
