using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Collector.Utility
{
    public class Syndication
    {
        //used to download & parse RSS & Atom feeds

        private Core S;

        public struct SyndicatedItem
        {
            public int index;
            public string url;
            public string title;
            public string summary;
            public string publisher;
            public DateTime datePublished;
            public List<string> images;
        }

        public Syndication(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public List<SyndicatedItem> Read(string feed)
        {
            var items = new List<SyndicatedItem>();
            var xml = S.Util.Xml.LoadXml(feed);
            if(xml.ChildNodes.Count > 0)
            {
                //var nodes = xml.SelectNodes("");
            }
            return items;
        }
    }
}
