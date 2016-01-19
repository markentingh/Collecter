using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Collector.Services
{
    public class Scavenger : Service
    {

        public struct ScavangedContent
        {
            public string url;
            public enumContentType type;
            public enumWebSearchEngines engine;
            public string html;
        }

        public enum enumContentType
        {
            text=0,
            webpage =1,
            pdf=2,
            ebook=3
        }

        public enum enumWebSearchEngines
        {
            all = 0,
            google = 1,
            bing = 2,
            duckduckgo = 3,
            wolfram = 4,
            youtube = 5,
            technorati = 6,
            livejournal = 7
        }

        private string[] searchEngines_All = { "google", "bing", "duckduckgo", "wolfram", "youtube", "technorati", "livejournal"};

        #region "Properties"
        private string googleCustomSearchID = "004475701512178542495:kqws5wpl_sm";
        private string googleAppKey = "AIzaSyADJblZ7ELWtDPzQMCWDOy2vLxEdklyz5Q";
        #endregion



        public Scavenger(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }
        #region "Scavenge Web Search"

        public List<ScavangedContent> GetContentFromWebSearch(string search = "", int maxResults = 20, enumWebSearchEngines searchType = enumWebSearchEngines.google)
        {
            var results = new List<ScavangedContent>();
            var html = "";
            //load list of search engines
            var engines = new string[] {};
            if(searchType != enumWebSearchEngines.all)
            {
                engines = new string[] { searchEngines_All[(int)searchType - 1] };
            }

            foreach(var engine in engines) {
                //get top 100 results from all search engines
                switch (engine)
                {
                    case "google":
                        
                        for(int x = 1; x <= maxResults / 10; x++)
                        {
                            //add 10 results at a time from google
                            html = SearchFromGoogle(search, ((x-1) * 10) + 1);
                            results.AddRange(GetUrlsFromWebSearch(html, enumWebSearchEngines.google));
                        }
                        
                        break;
                }
            }

            //download HTML for each URL in the results list
            for(var x = 0; x < results.Count; x++)
            {
                var result = results[x];
                result.html = S.Util.Web.DownloadFromPhantomJS(result.url);
            }
            return results;
        }

        public string SearchFromGoogle(string search = "", int startIndex = 1)
        {
            //get results as JSON from Google Custom Search API
            return S.Util.Web.Download(String.Format("https://www.googleapis.com/customsearch/v1?key={0}&cx={1}&q={2}&alt=json&start=" + startIndex, googleAppKey, googleCustomSearchID, search));
        }

        public List<ScavangedContent> GetUrlsFromWebSearch(string html, enumWebSearchEngines searchType)
        {
            var urls = new List<ScavangedContent>();
            switch (searchType)
            {
                case enumWebSearchEngines.google:
                    //parse Google search results
                    
                    break;
            }
            return urls;
        }

        #endregion
    }
}
