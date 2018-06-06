using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Strings;
using Utility.DOM;
using Collector.Models.Article;

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

        public static string Download(string url)
        {
            var path = Server.MapPath(Server.Instance.Cache["browserPath"].ToString());

            //execute WebBrowser console app to get DOM results from offscreen Chrome browser
            return Utility.Shell.Execute(path, "-url " + url, path.Replace("WebBrowser.exe",""), 60);
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

        public static string RenderArticle(AnalyzedArticle article)
        {
            var html = new StringBuilder();
            var parts = new List<ArticlePart>();
            DomElement elem;
            List<string> hierarchyTags;
            var lastHierarchy = new int[] { };
            int index;
            var newline = false;
            var fontsize = 1;
            var baseFontSize = 16;
            var incFontSize = 2.0; //font size increment between header tags
            var maxFontSize = 20;
            var fontsizes = new List<KeyValuePair<int, int>>();
            var isBold = false;
            var isItalic = false;


            //get all font sizes from all text to determine base font-size
            for (var x = 0; x < article.body.Count; x++)
            {
                fontsize = 0;
                index = article.body[x];
                elem = article.elements[index];
                if(elem.style != null)
                {
                    if (elem.style.ContainsKey("font-size"))
                    {
                        try { fontsize = int.Parse(elem.style["font-size"].Replace("px", "")); } catch (Exception) { }
                    }
                    if (fontsize > 0)
                    {
                        index = fontsizes.FindIndex(a => a.Key == fontsize);
                        if (index >= 0)
                        {
                            fontsizes[index] = new KeyValuePair<int, int>(fontsize, fontsizes[index].Value + 1);
                        }
                        else
                        {
                            fontsizes.Add(new KeyValuePair<int, int>(fontsize, 1));
                        }
                    }
                }
                
            }
            //sort font sizes & get top font size as base font size
            fontsizes = fontsizes.OrderBy(a => a.Value * -1).ToList();
            baseFontSize = fontsizes[0].Key;
            foreach(var size in fontsizes)
            {
                if(size.Key > maxFontSize && (size.Key - baseFontSize) <= 20) {
                    maxFontSize = size.Key;
                }
            }
            incFontSize = (maxFontSize - baseFontSize) / 6.0;

            //generate article parts from DOM
            for (var x = 0; x < article.body.Count; x++)
            {

                index = article.body[x];
                elem = article.elements[index];
                hierarchyTags = elem.HierarchyTags().ToList();
                newline = false;
                isBold = false;
                isItalic = false;
                fontsize = 1;

                var part = new ArticlePart();
                part.value = elem.text;

                if (elem.style != null)
                {
                    //determine font styling (bold & italic)
                    if (elem.style.ContainsKey("font-weight")) { isBold = elem.style["font-weight"] == "bold"; }
                    if (elem.style.ContainsKey("font-style")) { isItalic = elem.style["font-style"] == "italic"; }

                    //determine font size
                    if (elem.style.ContainsKey("font-size")) {
                        fontsize = int.Parse(elem.style["font-size"].Replace("px", ""));
                    }
                    fontsize = fontsize - baseFontSize;
                    if (fontsize > 1) {
                        fontsize = (int)Math.Round(fontsize / incFontSize);
                    }
                    if(fontsize > 6) { fontsize = 6; }
                    if(fontsize < 1) { fontsize = 1; }
                }
                part.fontSize = fontsize;

                //check last hierarchy for line break
                if (x > 0 && lastHierarchy.Count() > 0)
                {
                    var y = 0;
                    var past = false;
                    for (y = 0; y < elem.hierarchyIndexes.Count(); y++)
                    {
                        if (y < lastHierarchy.Length)
                        {
                            if (elem.hierarchyIndexes[y] != lastHierarchy[y])
                            {
                                past = true;
                            }
                        }
                        else
                        {
                            past = true;
                        }
                        if(past == true)
                        {
                            //found first item in element hierarchy that is different from last element's hierarchy
                            var baseElem = article.elements[elem.hierarchyIndexes[y]];
                            if (baseElem.style != null)
                            {
                                if (baseElem.style.ContainsKey("display"))
                                {
                                    //determine new line by element display property
                                    if (baseElem.style["display"] == "block")
                                    {
                                        newline = true;
                                    }
                                }
                            }

                            if (newline == false)
                            {
                                //determine new line by element tag
                                newline = Rules.blockElements.Contains(baseElem.tagName.ToLower());
                            }
                            break;
                        }
                    }

                    for (y = 0; y < elem.hierarchyIndexes.Count(); y++)
                    {
                        //check for specific tags within element hierarchy
                        var baseElem = article.elements[elem.hierarchyIndexes[y]];

                        //check for anchor link
                        if (baseElem.tagName == "a")
                        {
                            if (baseElem.attribute != null)
                            {
                                if (baseElem.attribute.ContainsKey("href"))
                                {
                                    var url = baseElem.attribute["href"];
                                    if (url.IndexOf("javascript:") < 0)
                                    {
                                        part.title = elem.text;
                                        part.value = url;
                                        part.type = TextType.anchorLink;
                                        break;
                                    }
                                }
                            }

                        }

                        switch (baseElem.tagName)
                        {
                            //check for list
                            case "ul":
                            case "ol":
                                part.type = TextType.listItem;
                                part.indent += 1;
                                break;

                            //check for headers
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                            case "h6":
                            case "title":
                                part.type = TextType.header;
                                break;
                        }
                    }
                }
                if (newline == true)
                {
                    parts.Add(new ArticlePart());
                }

                parts.Add(part);
                lastHierarchy = elem.hierarchyIndexes;
            }

            //render HTML from article parts
            var paragraph = false;
            var indent = 0;
            foreach(var part in parts)
            {
                //create paragraph tag (if neccessary)
                if (paragraph == false)
                {
                    switch (part.type)
                    {
                        case TextType.mainArticle:
                        case TextType.anchorLink:
                        case TextType.listItem:
                            paragraph = true;
                            html.Append("<p>");
                            break;
                    }
                }
                else
                {
                    switch (part.type)
                    {
                        case TextType.header:
                            paragraph = false;
                            html.Append("</p>");
                            break;
                    }
                }

                //escape list if neccessary
                switch (part.type)
                {
                    case TextType.listItem: break;
                    default:
                        for(var x = 1; x <= indent; x++)
                        {
                            html.Append("</ul>");
                        }
                        indent = 0;
                        break;
                }

                //render contents of article part
                switch (part.type)
                {
                    case TextType.mainArticle:
                        if(part.value == "" && paragraph == true)
                        {
                            paragraph = false;
                            html.Append("</p>");
                        }
                        else
                        {
                            html.Append("<span class=\"font-" + part.fontSize + "\">" + part.value + "</span>\n");
                        }
                        break;
                    case TextType.header:
                        html.Append("<h" + (7 - part.fontSize) + ">" + part.value + "</h" + (7 - part.fontSize) + ">\n");
                        break;
                    case TextType.anchorLink:
                        html.Append("<a href=\"" + part.value + "\" target=\"_blank\">" + part.title + "</a>\n");
                        break;
                    case TextType.listItem:
                        if(part.indent > indent)
                        {
                            for(var x = indent; x < part.indent; x++)
                            {
                                html.Append("<ul>");
                            }
                            indent = part.indent;
                        }
                        else if(part.indent < indent)
                        {
                            for (var x = part.indent; x > indent; x--)
                            {
                                html.Append("</ul>");
                            }
                            indent = part.indent;
                        }
                        html.Append("<li>" + part.value + "</li>");
                        break;
                }
            }

            return html.ToString();
        }
    }
}
