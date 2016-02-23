using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

namespace Collector.Services
{
    public class Downloads : Service
    {
        public struct structDownloadInfo
        {
            public int queueId;
            public int feedId;
            public string url;
            public int status;
        }

        public Downloads(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        #region "Servers"
        public Inject AddServer(int type, string title, string settings)
        {
            var response = new Inject();
            var serverTitle = title;
            switch (type)
            {
                case 0: //local
                    //first, make sure the local server doesn't already exist
                    if((int)S.Sql.ExecuteScalar("EXEC DownloadServerExists @settings='" + settings + "'") == 0)
                    {
                        serverTitle = "Local Host";
                        S.Sql.ExecuteNonQuery("EXEC AddDownloadServer @type=" + type + ", @title='" + serverTitle + "', @settings='" + settings + "'");
                    }
                    else
                    {
                        S.Page.RegisterJS("err", "alert('You have already added your local server to the server list.');");
                    }
                    break;

                case 1: //web server

                    break;
            }

            response.inject = enumInjectTypes.replace;
            response.element = ".server-list .contents";
            response.html = LoadServersUI();
            response.js = CompileJs();
            return response;
        }

        public string LoadServersUI()
        {
            var htm = "";
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetDownloadServers"));
            if(reader.Rows.Count > 0)
            {
                var i = 0;
                while (reader.Read())
                {
                    htm += "<div class=\"row server server" + i + "\">" +
                        
                        //settings button
                        "<div class=\"btn\"><a href=\"javascript:\" onclick=\"S.downloader.buttons.serverSettings(" + i + ")\" class=\"button green\">Settings</a></div>" +

                        //downloading iframes
                        "<div class=\"downloading\"></div>" +

                        //title & url
                        "<div class=\"title\">" + reader.Get("title") + "</div>" +
                        "<div class=\"settings\">" + reader.Get("settings") + "</div>" + 
                        "</div>";
                    i++;
                }
            }

            return htm;
        }

        public string LoadQueueUI()
        {
            var htm = "";
            var downloadCount = S.Sql.ExecuteScalar("EXEC GetDownloadCount");
            htm = "<div class=\"row\"><div class=\"column title\">Downloads in Queue:</div>" +
                "<div class=\"column total\">" + String.Format("{0:N0}", downloadCount) + "</div>";
            S.Page.RegisterJS("dlcount", "setTimeout(function(){S.downloader.totalDownloads = " + downloadCount + ";},10);");
            return htm;
        }
        #endregion

        #region "Downloader"
        public Inject StartDownloads()
        {
            var response = new Inject();

            //get list of servers
            var reader = new SqlReader();
            var i = 0;
            var js = "";
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetDownloadServers"));
            
            if (reader.Rows.Count > 0)
            {
                while (reader.Read())
                {
                    //create distribution lists
                    S.Sql.ExecuteNonQuery("EXEC AddDownloadDistribution @serverId=" + reader.GetInt("serverId"));

                    //inject iframes for each server
                    js += "S.downloader.loadServerFrames(" + i + ", " + reader.GetInt("serverId") + ", '" + reader.Get("settings") + "');\n";
                    i++;
                }
                S.Page.RegisterJS("startdl", js);
            }
            response.element = "";
            response.html = "";
            response.js = CompileJs();
            return response;
        }

        public Inject Download(int index)
        {
            var response = new Inject();
            var list = new List<structDownloadInfo>();
            var nextIndex = index + 1;
            var minusIndex = 1;
            var serverId = 0;
            //get download server Id
            if (S.Session.Keys.Contains("downloadServerId"))
            {
                serverId = int.Parse((string)S.Util.Serializer.ReadObject(S.Util.Str.GetString(S.Session.Get("downloadServerId")), typeof(string)));
            }
            else{
                serverId = (int)S.Sql.ExecuteScalar("EXEC GetDownloadServerId @host='" + S.Request.Host.ToString() + "'");
                S.Session.Set("downloadServerId", S.Util.Str.GetBytes(serverId.ToString()));
            }

            //get download distribution list for this server
            if (S.Session.Keys.Contains("downloadQueue"))
            {
                list = (List<structDownloadInfo>)S.Util.Serializer.ReadObject(S.Util.Str.GetString(S.Session.Get("downloadQueue")), typeof(List<structDownloadInfo>));
                if(index < list.Count)
                {
                    //download next html web page using PhantomJS) in the list
                    var url = list[index].url;
                    var d = S.Util.Web.DownloadFromPhantomJS(url);
                    if(d.html.Length > 0)
                    {
                        url = d.url;
                        var html = d.html;
                        var title = "";
                        var articles = new Articles(S, S.Page.Url.paths);
                        var article = articles.SetupAnalyzedArticle(url, html);

                        //set feedId for article
                        article.feedId = list[index].feedId;

                        //get web page title
                        var titleStart = html.IndexOf("<title>");
                        if (titleStart >= 0)
                        {
                            var titleEnd = html.IndexOf("</title>");
                            if (titleEnd > 0)
                            {
                                //found title
                                titleStart += 7;
                                title = html.Substring(titleStart, titleEnd - titleStart);
                            }
                        }
                        if (title == "") { title = "Unknown Title from " + S.Util.Str.GetDomainName(url); }
                        article.pageTitle = title;
                        article.title = title;
                        var id = S.Sql.ExecuteScalar("EXEC GetArticleByUrl @url='" + article.url + "'");
                        if(!S.Util.IsEmpty(id))
                        {
                            article.id = (int)id;
                        }
                        
                        //save web page as new article to "success"
                        articles.SaveArticle(article);

                        //update download queue
                        S.Sql.ExecuteNonQuery("EXEC UpdateDownload @qid=" + list[index].queueId + ", @status=1");
                    }
                    else
                    {
                        //update download queue status to "fail"
                        S.Sql.ExecuteNonQuery("EXEC UpdateDownload @qid=" + list[index].queueId + ", @status=2");
                    }
                }
                else
                {
                    //get a new distribution list for this server
                    nextIndex = 0;
                    minusIndex = 0;
                    S.Sql.ExecuteNonQuery("EXEC AddDownloadDistribution @serverId=" + serverId);
                    LoadDistributionList(serverId);
                }

                //prep javascript to request next download in distribution list
                S.Page.RegisterJS("dl", "S.downloads.download(" + nextIndex + ", " + minusIndex + ");");
            }
            response.html = "";
            response.js = CompileJs();
            return response;
        }

        public void LoadDistributionList(int serverId)
        {
            //get download distribution list for this server
            var reader = new SqlReader();
            var list = new List<structDownloadInfo>();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetDownloadDistributionList @serverId=" + serverId));
            if (reader.Rows.Count > 0)
            {
                while (reader.Read())
                {
                    var dl = new structDownloadInfo();
                    dl.queueId = reader.GetInt("qid");
                    dl.url = reader.Get("url");
                    dl.status = 0;
                    dl.feedId = reader.GetInt("feedId");
                    list.Add(dl);
                }
            }
            S.Session.Set("downloadQueue", S.Util.Serializer.WriteObject(list));
        }
        #endregion
    }
}
