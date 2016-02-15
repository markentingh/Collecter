using System.Collections.Generic;
using Microsoft.AspNet.Http;
using System.Linq;

namespace Collector.Services
{
    public class Feeds : Service
    {

        private struct structFeedList
        {
            public int id;
            public string url;
        }

        #region "Feeds"
        public Feeds(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        public Inject AddFeed(string title, string url)
        {
            var inject = new Inject();

            //save new feed into database
            S.Sql.ExecuteNonQuery("EXEC AddFeed @title='" + S.Sql.Encode(title) + "', @url='" + S.Sql.Encode(url) + "'");
            S.Page.RegisterJS("addfeed", "alert('Feed added successfully');");

            //setup response
            inject.element = ".feeds .contents";
            inject.html = LoadFeedsUI();
            inject.js = CompileJs();
            return inject;
        }

        public Inject CheckFeeds()
        {
            //start the process to check all feeds for new urls
            var inject = new Inject();

            //get list of feeds
            var js = "";
            var reader = new SqlReader();
            var feeds = new List<structFeedList>();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetFeeds"));
            if (reader.Rows.Count > 0)
            {
                js += "S.feeds.list = [";
                var i = 0;
                while (reader.Read())
                {
                    if (i > 0) { js += ","; }
                    js += "'" + reader.Get("url") + "'";
                    var newfeed = new structFeedList();
                    newfeed.id = reader.GetInt("feedid");
                    newfeed.url = reader.Get("url");
                    feeds.Add(newfeed);
                    i++;
                }
                js += "];";
            }

            //save feeds list to session
            S.Session.Set("feedlist",S.Util.Serializer.WriteObject(feeds));

            //start client-side task to check each feed
            js += "S.feeds.buttons.checkFeed(0);";
            inject.js = js;
            return inject;
        }

        public Inject CheckFeed(int index, bool checkMore)
        {
            //check a specific feed for new urls
            var inject = new Inject();
            var js = "";
            var feeds = new List<structFeedList>();
            if (S.Session.Keys.Contains("feedlist") == true)
            {
                feeds = (List<structFeedList>)S.Util.Serializer.ReadObject(S.Util.Str.GetString(S.Session.Get("feedlist")), feeds.GetType());
                if(index == feeds.Count && checkMore == true)
                {
                    //no more feeds to check
                    js += "S.feeds.checkedAllFeeds();";
                }
                else
                {
                    //add urls from feed to download queue
                    var total = ReadFeedFromUrl(feeds[index].url);

                    //log feed check
                    S.Sql.ExecuteNonQuery("EXEC AddFeedCheckedLog @feedId=" + feeds[index].id + ", @links=" + total);

                    js += "S.feeds.updateFeedStatus('" + S.Util.Str.GetSubDomainAndDomain(feeds[index].url) + "'," + total + ", " + index + ");";
                    if (checkMore == true) {
                        //check next feed
                        js += "S.feeds.buttons.checkFeed(" + (index + 1) + ");";
                    }
                    else
                    {
                        //check only one feed
                        js += "S.feeds.checkedFeed('" + S.Util.Str.GetSubDomainAndDomain(feeds[index].url) + "');";
                    }
                }
            }
            inject.js = js;
            return inject;
        }

        public int ReadFeedFromUrl(string url)
        {
            return ReadFeed(S.Util.Web.Download(url));
        }

        public int ReadFeed(string html)
        {
            var feed = S.Util.RSS.Read(html);
            var total = 0;
            foreach (var item in feed.items)
            {
                if (AddToDownloadQueue(item.link) == true)
                {
                    total++;
                }
            }
            return total;
        }
        
        public string LoadFeedsUI()
        {
            var htm = "";
            var js = "";
            var reader = new SqlReader();
            var feeds = new List<structFeedList>();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetFeeds"));
            if(reader.Rows.Count > 0)
            {
                js += "setTimeout(function(){ S.feeds.list = [";
                var i = 0;
                while (reader.Read())
                {
                    htm += "<div class=\"feed\">" +
                        "<div class=\"btn\"><a href=\"javascript:\" onclick=\"S.feeds.buttons.checkFeed(" + i + ")\" class=\"button green\">Check</a></div>" +
                        "<div class=\"title\">" + reader.Get("title") + "</div>" +
                        "<div class=\"url\">" + reader.Get("url") + "</div>" +
                        "</div>";

                    if (i > 0) { js += ","; }
                    js += "'" + reader.Get("url") + "'";
                    var newfeed = new structFeedList();
                    newfeed.id = reader.GetInt("feedid");
                    newfeed.url = reader.Get("url");
                    feeds.Add(newfeed);
                    i++;
                }
                js += "];}, 1000);";
                S.Page.RegisterJS("feedlist", js);
            }

            //save feeds list to session
            S.Session.Set("feedlist", S.Util.Serializer.WriteObject(feeds));

            return htm;
        }
        #endregion

        #region "Download Queue"
        public bool AddToDownloadQueue(string url, int feedId = 0)
        {
            if (IsUrlBlacklisted(url) == true) { return false; }
            return (int)S.Sql.ExecuteScalar("EXEC AddToDownloadQueue @url='" + S.Util.Str.CleanUrl(url) + "', @feedId=" + feedId) == 1 ? true : false;
        }

        public bool IsUrlBlacklisted(string url)
        {
            string[] blacklist;
            if(S.Server.Cache.ContainsKey("url-blacklist") == true)
            {
                //load blacklist from cache
                blacklist = (string[])S.Server.Cache["url-blacklist"];
            }
            else
            {
                //load blacklist from file
                blacklist = (string[])S.Util.Serializer.OpenFromFile(typeof(string[]), S.Server.MapPath("/content/blacklist.json"));
                S.Server.Cache.Add("url-blacklist", blacklist);
            }

            //check blacklist for domain
            var domain = S.Util.Str.GetDomainName(url);
            if (blacklist.Contains(domain))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
