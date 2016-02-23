using System;
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

        public struct structFeedLogData
        {
            public int count;
            public DateTime date;
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
                    var total = ReadFeedFromUrl(feeds[index].url, feeds[index].id);

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

        public int ReadFeedFromUrl(string url, int feedId)
        {
            return ReadFeed(S.Util.Web.Download(url), feedId);
        }

        public int ReadFeed(string html, int feedId)
        {
            var feed = S.Util.RSS.Read(html);
            var total = 0;
            foreach (var item in feed.items)
            {
                if (AddToDownloadQueue(item.link, feedId) == true)
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
            var logdata = new List<structFeedLogData>();
            var days = 5;
            var feedId = 0;
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetFeedsAndLogs @dateStart='" + DateTime.Now.AddDays(0-(days-1)).ToString("yyyy-MM-dd HH:mm:ss") + "', @days=" + days));
            if(reader.Rows.Count > 0)
            {
                js += "setTimeout(function(){ S.feeds.list = [";
                var i = 0;
                while (reader.Read())
                {
                    if(reader.Get("title") != "")
                    {
                        //new feed
                        feedId = reader.GetInt("feedid");
                        if (i > 0)
                        {
                            //render log data chart
                            htm = htm.Replace("{{chart}}", GetFeedChartFromData(feedId, days, logdata)) + "</div>";
                            
                        }
                        htm += "<div class=\"row feed feed" + i + "\">" +
                        
                        //check button
                        "<div class=\"btn\"><a href=\"javascript:\" onclick=\"S.feeds.buttons.checkFeed(" + i + ")\" class=\"button green\">Check</a></div>" +
                        
                        //include chart
                        "{{chart}}" +
                        
                        //title & url
                        "<div class=\"title\">" + reader.Get("title") + "</div>" +
                        "<div class=\"url\">" + reader.Get("url") + "</div>";

                        if (i > 0) { js += ","; }
                        js += "'" + reader.Get("url") + "'";
                        var newfeed = new structFeedList();
                        newfeed.id = feedId;
                        newfeed.url = reader.Get("url");
                        feeds.Add(newfeed);

                        logdata = new List<structFeedLogData>();
                        i++;
                    }
                    else
                    {
                        //add log data for chart
                        var newlog = new structFeedLogData();
                        newlog.count = reader.GetInt("loglinks");
                        newlog.date = reader.GetDateTime("logdatechecked");
                        logdata.Add(newlog);
                    }
                }
                i--;
                //render log data chart for last feed item
                if (i >= 0) { htm = htm.Replace("{{chart}}", GetFeedChartFromData(feedId, days, logdata)) + "</div>"; }

                js += "];}, 1000);";
                S.Page.RegisterJS("feedlist", js);
            }

            //save feeds list to session
            S.Session.Set("feedlist", S.Util.Serializer.WriteObject(feeds));

            return htm;
        }

        public string GetFeedChartFromData(int feedId, int days, List<structFeedLogData> logData)
        {
            var htm = ""; var js = "";
            var markerLeft = new int[2] { 999, 0 };
            var dateend = new DateTime(2001,1,1);
            var daynames = new string[3] { "", "", "" };
            var minCount = 0;
            var maxCount = 0;
            var rangeCount = 0;
            var hours = 0.0;
            var xhours = 0.0;
            var ylinks = 0.0;
            var i = 0;
            var dataTime = new TimeSpan();
            
            //get date range
            foreach (var data in logData)
            {
                if(data.count < minCount) { minCount = data.count; }
                if (data.count > maxCount) { maxCount = data.count; }
                if (data.date > dateend) { dateend = data.date; }
            }
            markerLeft[0] = 0;
            markerLeft[1] = maxCount;
            rangeCount = maxCount - minCount;

            if(maxCount > 0)
            {
                //add javascript that will render the chart
                js = "setTimeout(function(){S.feeds.loadChart(" + feedId + ", [";
                foreach (var data in logData)
                {
                    dataTime = (dateend - data.date);
                    hours = dataTime.TotalHours;
                    xhours = Math.Round((100.0 / (days * 24.0)) * ((days * 24.0) - hours));
                    ylinks = (maxCount == 0 ? 18 : 36 - Math.Round((36.0 / maxCount) * data.count));
                    js += (i > 0 ? "," : "") + "[" + xhours + "," + ylinks + "]";
                    i++;
                }

                js += "]);},10);"; //end of S.feeds.loadChart();
            }

            //get day names
            daynames[0] = dateend.AddDays(1 - days).ToString("ddd").ToLower();
            daynames[1] = dateend.AddDays(Math.Round(days / 2.0) * -1).ToString("ddd").ToLower();
            daynames[2] = dateend.ToString("ddd").ToLower();

            
            htm +=  "<div class=\"chart\">" +

            //add 3 vertical markers to represent lowest & highest link count
                    "<div class=\"marker-left marker1\">" + markerLeft[0] + "</div>" +
                    "<div class=\"marker-left marker2\"></div>" +
                    "<div class=\"marker-left marker3\">" + markerLeft[1] + "</div>" +

            //add 3 horizontal markers to represent first, middle & last day of week
                    "<div class=\"marker-bottom markerb1\">" + daynames[0] + "</div>" +
                    "<div class=\"marker-bottom markerb2\">" + daynames[1] + "</div>" +
                    "<div class=\"marker-bottom markerb3\">" + daynames[2] + "</div>" +
                    
            //show raphael js paper
                    "<div id=\"paperfeed" + feedId + "\"></div>" +

                    "</div>";

            S.Page.RegisterJS("feedchart" + feedId, js);

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
