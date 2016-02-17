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

        public struct SyndicatedFeed
        {
            public string title;
            public string description;
            public string link;
            public string copyright;
            public string language;
            public string feedUrl;
            public SyndicatedItemImage image;
            public List<SyndicatedItem> items;
        }

        public struct SyndicatedItem
        {
            public int index;
            public string link;
            public string title;
            public string description;
            public string publisher;
            public DateTime publishDate;
            public List<string> images;
        }

        public struct SyndicatedItemImage
        {
            public string title;
            public string url;
            public string link;
            public int width;
            public int height;
        }


        public Syndication(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public SyndicatedFeed Read(string html)
        {
            var feed = new SyndicatedFeed();
            var items = new List<SyndicatedItem>();
            //use DOM parser instead of System.Xml
            var dom = new DOM.Parser(S, html);
            var elements = dom.Find("//channel");
            List<DOM.DomElement> elems;
            //get channel info
            if (elements.Count > 0)
            {
                var channel = elements[0];
                
                //get channel title
                elems = channel.Find("/title");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            feed.title = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                    }
                }

                //get channel description
                elems = channel.Find("/description");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            feed.description = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                    }
                }

                //get channel link
                elems = channel.Find("/link");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            feed.link = S.Util.Str.HtmlDecode(elems[0].FirstChild.text);
                        }
                        catch (Exception ex) { }
                    }
                }

                //get channel language
                elems = channel.Find("/language");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            feed.language = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                    }
                }

                //get channel copyright
                elems = channel.Find("/copyright");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            feed.copyright = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                    }
                }

                //get channel image properties
                elems = channel.Find("/image");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        var img = elems[0];
                        var image = new SyndicatedItemImage();

                        //get channel image title
                        elems = img.Find("/title");
                        if (elems.Count > 0)
                        {
                            if (elems[0].FirstChild != null)
                            {
                                try {
                                    image.title = elems[0].FirstChild.text;
                                }
                                catch (Exception ex) { }
                            }
                        }

                        //get channel image url
                        elems = img.Find("/url");
                        if (elems.Count > 0)
                        {
                            if (elems[0].FirstChild != null)
                            {
                                try {
                                    image.url = S.Util.Str.HtmlDecode(elems[0].FirstChild.text);
                                }
                                catch (Exception ex) { }
                            }
                        }

                        //get channel image link
                        elems = img.Find("/link");
                        if (elems.Count > 0)
                        {
                            if (elems[0].FirstChild != null)
                            {
                                try {
                                    image.link = S.Util.Str.HtmlDecode(elems[0].FirstChild.text);
                                }
                                catch (Exception ex) { }
                            }
                        }

                        //get channel image width
                        elems = img.Find("/width");
                        if (elems.Count > 0)
                        {
                            if (elems[0].FirstChild != null)
                            {
                                if (S.Util.Str.IsNumeric(elems[0].FirstChild.text))
                                {
                                    try {
                                        image.width = int.Parse(elems[0].FirstChild.text);
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                        }

                        //get channel image height
                        elems = img.Find("/height");
                        if (elems.Count > 0)
                        {
                            if (elems[0].FirstChild != null)
                            {
                                if (S.Util.Str.IsNumeric(elems[0].FirstChild.text))
                                {
                                    try {
                                        image.height = int.Parse(elems[0].FirstChild.text);
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                        }
                        feed.image = image;
                    }
                }

                //get channel copyright
                elems = channel.Find("/atom:link");
                if (elems.Count > 0)
                {
                    if (S.Util.IsEmpty(elems[0].attribute["href"]) == false)
                    {
                        try {
                            feed.feedUrl = S.Util.Str.HtmlDecode(elems[0].attribute["href"]);
                        }
                        catch (Exception ex) { }
                    }
                }
            }
            elements = dom.Find("//item");
            foreach(var element in elements)
            {
                var item = new SyndicatedItem();

                //get title
                elems = element.Find("/title");
                if(elems.Count > 0)
                {
                    if(elems[0].FirstChild != null)
                    {
                        try {
                            item.title = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                    }
                }

                //get url link
                elems = element.Find("/link");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try {
                            item.link = S.Util.Str.HtmlDecode(elems[0].FirstChild.text);
                        }
                        catch (Exception ex) { }
                    }
                }

                //get url link from atom:link
                elems = element.Find("/atom:link");
                if (elems.Count > 0)
                {
                    if (S.Util.IsEmpty(elems[0].attribute["href"]) == false)
                    {
                        try {
                            item.link = S.Util.Str.HtmlDecode(elems[0].attribute["href"]);
                        }
                        catch (Exception ex) { }
                    }
                }

                //get description
                elems = element.Find("/description");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try
                        {
                            item.description = elems[0].FirstChild.text;
                        }
                        catch (Exception ex) { }
                        
                    }
                }

                //get publish date
                elems = element.Find("/pubDate");
                if (elems.Count > 0)
                {
                    if (elems[0].FirstChild != null)
                    {
                        try
                        {
                            item.publishDate = DateTime.Parse(elems[0].FirstChild.text);
                        }
                        catch(Exception ex) { }
                    }
                }
                items.Add(item);

            }
            feed.items = items;
            return feed;
        }
    }
}
