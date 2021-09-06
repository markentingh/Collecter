using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using Collector.Common.Platform;
using Collector.Common.Analyze;
using Collector.Models.Article;
using Utility.Strings;

namespace Collector.SignalR.Hubs
{
    public class DownloadHub : Hub
    {
        public async Task CheckQueue()
        {
            var queue = Query.Downloads.CheckQueue();
            if(queue != null)
            {
                AnalyzedArticle article = new AnalyzedArticle();
                await Clients.Caller.SendAsync("update", "Downloading <a href=\"" + queue.url + "\" target=\"_blank\">" + queue.url + "</a>...");

                //download content
                var result = Article.Download(queue.url);
                if (result == "")
                {
                    await Clients.Caller.SendAsync("update", "Download timed out for URL: <a href=\"" + queue.url + "\" target=\"_blank\">" + queue.url + "</a>");
                    return;
                }
                try
                {
                    article = Html.DeserializeArticle(result);
                }
                catch (Exception)
                {
                    await Clients.Caller.SendAsync("update", "Error parsing DOM!");
                    await Clients.Caller.SendAsync("checked");
                    return;
                }
                //save article
                Article.Add(queue.url);

                //get URLs from all anchor links on page
                var urls = new Dictionary<string, List<string>>();
                var links = article.elements.Where(a => a.tagName == "a").Select(a => a.attribute.ContainsKey("href") ? a.attribute["href"] : "");
                foreach(var url in links)
                {
                    if (string.IsNullOrEmpty(url)) { continue; }
                    var uri = Web.CleanUrl(url);
                    if (uri.StartsWith("mailto:")) { continue; }
                    if (uri.StartsWith("javascript:")) { continue; }
                    var domain = uri.GetDomainName();
                    if (Models.Blacklist.Domains.Any(a => domain.IndexOf(a) == 0)) { continue; }
                    if (!urls.ContainsKey(domain))
                    {
                        urls.Add(domain, new List<string>());   
                    }
                    urls[domain].Add(uri);
                }
                foreach(var domain in urls.Keys)
                {
                    var count = Query.Downloads.AddQueueItems(string.Join(",", urls[domain].ToArray()), domain, queue.feedId);
                    if(count > 0)
                    {
                        await Clients.Caller.SendAsync("update", 
                            "<span>Found " + count + " new link(s) for domain " + domain + "</span>" +
                            " <div class=\"col right\"><a href=\"javascript:\" onclick=\"S.downloads.blacklist.add('" + domain + "')\"><small>blacklist domain</small></div>");
                    }
                    
                }

            }
            await Clients.Caller.SendAsync("checked");
        }

        public async Task CheckFeeds()
        {
            var feeds = Query.Feeds.Check();
            foreach(var feed in feeds)
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString(feed.url);
                    var content = Utility.Syndication.Read(response);
                    var links = content.items.Select(a => a.link);
                    var domains = new Dictionary<string, List<string>>();
                    //separate links by domain
                    foreach(var link in links)
                    {
                        var domain = link.GetDomainName();
                        if (!domains.ContainsKey(domain))
                        {
                            domains.Add(domain, new List<string>());
                        }
                        domains[domain].Add(link);
                    }
                    //add all links for all domains to download queue
                    var count = 0;
                    foreach(var domain in domains.Keys)
                    {
                        var dlinks = domains[domain];
                        if (dlinks.Count > 0)
                        {
                            count += Query.Downloads.AddQueueItems(string.Join(",", dlinks), domain);
                        }
                    }
                    if(count > 0)
                    {
                        await Clients.Caller.SendAsync("update", "Added " + count + " URLs to the download queue from feed " + feed.url);
                    }
                }
                Query.Feeds.UpdateLastChecked(feed.feedId);
            }
            await Clients.Caller.SendAsync("update", "Checked feeds.");
        }

        public async Task Blacklist(string domain)
        {
            Query.Blacklists.Domains.Add(domain);
            try
            {
                //delete physical content for domain on disk
                Directory.Delete(App.MapPath("/Content/" + domain.Substring(0, 2) + "/" + domain), true);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("update", "Could not delete folder " + "/Content/" + domain.Substring(0, 2) + "/" + domain);
            }
            //add domain to black list object
            if (!Models.Blacklist.Domains.Contains(domain))
            {
                Models.Blacklist.Domains.Add(domain);
            }
            await Clients.Caller.SendAsync("update", "Blacklisted domain " + domain + " and removed all related articles");
        }
    }
}
