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

        public static double Version
        {
            get
            {
                var ver = Server.Instance.Version.Split('.');
                return double.Parse(ver[0] + "." + string.Join("", ver.Skip(1)));
            }
        }

        public static string Download(string url)
        {
            var path = Server.MapPath(Server.Instance.Cache["browserPath"].ToString());

            //execute WebBrowser console app to get DOM results from offscreen Chrome browser
            return Utility.Shell.Execute(path, "-url " + url, path.Replace("WebBrowser.exe",""), 60);
        }

        public static void FileSize(AnalyzedArticle article)
        {
            article.fileSize = int.Parse((Encoding.Unicode.GetByteCount(article.rawHtml) / 1024).ToString("c").Replace("$", "").Replace(",", "").Replace(".00", ""));
        }

        public static Query.Models.Article Add(string url)
        {
            var version = Version;
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
            var indent = 0;
            var indents = new List<int>();
            var indentOpen = false;
            var inQuote = 0;
            var hasQuote = false;
            var listItem = 0;
            var fontsize = 1;
            var baseFontSize = 16;
            var incFontSize = 2.0; //font size increment between header tags
            var maxFontSize = 20;
            var fontsizes = new List<KeyValuePair<int, int>>();
            var isBold = false;
            var isItalic = false;
            var relpath = ContentPath(article.url).ToLower();
            ArticlePart lastPart = new ArticlePart();


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
                hasQuote = false;

                if(elem.tagName == "br") {
                    parts.Add(new ArticlePart() {
                        type = new List<TextType>() { TextType.lineBreak }
                    });
                    continue;
                }
                if(elem.tagName == "img")
                {
                    var img = article.images.Find(a => a.index == elem.index);
                    if(img != null)
                    {
                        if (img.exists == true)
                        {
                            parts.Add(new ArticlePart()
                            {
                                value = relpath + article.id + "/" + img.index + "." + img.extension,
                                type = new List<TextType>() { TextType.image }
                            });
                        }
                    }
                }
                if(elem.text == "" || elem.text == null) { continue; }

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
                if (x > 0 && lastHierarchy.Length > 0)
                {
                    var y = 0;
                    var past = false;
                    for (y = 0; y < elem.hierarchyIndexes.Length; y++)
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
                        if (past == true)
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

                    //find base elements that are exceptions to new lines
                    if (newline == true && indents.Count > 0)
                    {
                        for (y = 0; y < elem.hierarchyIndexes.Length; y++)
                        {
                            var baseElem = article.elements[elem.hierarchyIndexes[y]];
                            switch (baseElem.tagName)
                            {
                                case "ul": case "ol": case "li":
                                    //base element is a list, which cannot contain new lines
                                    newline = false;
                                    break;
                            }
                            if(newline == false) { break; }
                        }
                    }
                    if(newline == true && indents.Count > 0)
                    {
                        indents = new List<int>();
                    }

                    for (y = 0; y < elem.hierarchyIndexes.Length; y++)
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
                                        part.type.Add(TextType.anchorLink);
                                        break;
                                    }
                                }
                            }
                        }

                        switch (baseElem.tagName)
                        {
                            //check for headers
                            case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                                part.type.Add(TextType.header);
                                part.fontSize = 7 - (int.Parse(baseElem.tagName.Replace("h", "")));
                                break;
                            case "title":
                                part.type.Add(TextType.header);
                                part.fontSize = 5;
                                break;
                        }

                        switch (baseElem.tagName)
                        {
                            //check for list
                            case "ul": case "ol":
                                if (!indents.Contains(baseElem.index))
                                {
                                    indents.Add(baseElem.index);
                                }
                                else
                                {
                                    var ix = indents.IndexOf(baseElem.index);
                                    if (ix >= 0 && ix < indents.Count - 1)
                                    {
                                        indents.RemoveRange(ix + 1, indents.Count - ix - 2);
                                    }
                                    else if (ix < 0)
                                    {
                                        indents = new List<int>() { baseElem.index };
                                    }
                                }
                                part.type.Add(TextType.listItem);
                                part.indent = indents.Count;

                                //find list item in element hierarchy
                                for (var z = elem.hierarchyIndexes.Length - 1; z > 0; z--)
                                {
                                    var liElem = article.elements[elem.hierarchyIndexes[z]];
                                    if (liElem.tagName == "li")
                                    {
                                        part.listItem = liElem.index;
                                        break;
                                    }
                                }
                                break;
                        }

                        switch (baseElem.tagName)
                        {
                            //check for quotes
                            case "blockquote":
                                part.type.Add(TextType.quote);
                                part.quote = baseElem.index;
                                inQuote = baseElem.index;
                                hasQuote = true;
                                break;
                        }
                    }
                }

                if(inQuote > 0 && hasQuote == false) { inQuote = 0; }
                if (newline == true)
                {
                    var nline = new ArticlePart()
                    {
                        type = new List<TextType>() { TextType.mainArticle },
                        quote = inQuote
                    };
                    parts.Add(nline);
                }

                //HTML encode content
                if(part.type.Where(a => a == TextType.mainArticle || a == TextType.header || a == TextType.listItem || a == TextType.quote).Count() > 0)
                {
                    part.value = part.value.Replace("&", "&amp;").Replace("<", "&lt;");
                }

                //finally, add part to render list
                if(part.type.Count == 0) { part.type.Add(TextType.mainArticle); }
                parts.Add(part);
                lastHierarchy = elem.hierarchyIndexes;
            }

            //render HTML from article parts
            var paragraph = false;
            indent = 0;
            inQuote = 0;
            hasQuote = false;

            foreach(var part in parts)
            {
                //create paragraph tag (if neccessary)
                if (paragraph == false && part.value != "" && indent == 0)
                {
                    if (part.type.Where(a => a == TextType.mainArticle || a == TextType.anchorLink).Count() > 0)
                    {
                        paragraph = true;
                        html.Append("<p>");
                    }
                }
                else if (part.value != "" && indent == 0)
                {
                    if (part.type.Where(a => a == TextType.header).Count() > 0)
                    {
                        paragraph = false;
                        html.Append("</p>");
                    }
                }

                //escape block quote if neccessary
                if (part.quote != inQuote && inQuote > 0)
                {
                    html.Append("</blockquote>");
                    inQuote = 0;
                }

                //escape list if neccessary
                if (part.type.Where(a => a == TextType.listItem || a == TextType.anchorLink || a == TextType.lineBreak).Count() == 0)
                { 
                    if(part.type.Where(a => a == TextType.mainArticle).Count() > 0)
                    {
                        if(part.value != "") { goto endEscape; }
                    }
                    if (indentOpen == true)
                    {
                        html.Append("</li>");
                        indentOpen = false;
                    }
                    for (var x = 1; x <= indent; x++)
                    {
                        html.Append("</ul>");
                    }
                    indent = 0;
                }

                endEscape:

                //render contents of article part
                var endTags = "";
                var showValue = false;
                var cancelValue = false;

                if (part.type.Where(a => a == TextType.listItem).Count() > 0)
                {
                    if (part.indent > indent)
                    {
                        //new unordered list
                        for (var x = indent; x < part.indent; x++)
                        {
                            html.Append("<ul>");
                        }
                        indent = part.indent;
                    }
                    else if (part.indent < indent)
                    {
                        //end of unordered list(s)
                        for (var x = part.indent; x > indent; x--)
                        {
                            html.Append("</ul>");
                        }
                        indent = part.indent;
                    }

                    //render list item
                    if (indentOpen == true && part.listItem != listItem)
                    {
                        html.Append("</li>");
                    }
                    if (listItem != part.listItem) { html.Append("<li>"); }
                    listItem = part.listItem;
                    indentOpen = true;
                    showValue = true;
                }

                if (part.type.Where(a => a == TextType.quote).Count() > 0)
                {
                    //render quote
                    if(inQuote > 0 && part.quote > inQuote)
                    {
                        html.Append("</blockquote>");
                    }
                    if(inQuote != part.quote)
                    {
                        html.Append("<blockquote>");
                    }
                    inQuote = part.quote;
                    showValue = true;
                }

                if (part.type.Where(a => a == TextType.anchorLink).Count() > 0)
                {
                    //render anchor link
                    html.Append("<a href=\"" + part.value + "\" target=\"_blank\"" +
                            (part.fontSize > 1 ? " class=\"font-" + part.fontSize + "\"" : "") +
                            ">" + part.title);
                    endTags += "</a>\n";
                    cancelValue = true;
                }

                if (part.type.Where(a => a == TextType.header).Count() > 0)
                {
                    //render header
                    html.Append("<h" + (7 - part.fontSize) + ">");
                    endTags += "</h" + (7 - part.fontSize) + ">\n";
                    showValue = true;
                }

                if (part.type.Where(a => a == TextType.mainArticle).Count() > 0)
                {
                    //render paragraph text
                    if (part.value == "" && paragraph == true)
                    {
                        //end of paragraph
                        paragraph = false;
                        html.Append("</p>");
                    }
                    else if (part.value != "")
                    {
                        html.Append("<span" +
                        (part.fontSize > 1 ? " class=\"font-" + part.fontSize + "\"" : "") +
                        ">");
                        endTags += "</span>\n";
                        showValue = true;
                    }
                }
                if (part.type.Where(a => a == TextType.lineBreak).Count() > 0)
                {
                    html.Append("<br/>");
                }
                if (part.type.Where(a => a == TextType.image).Count() > 0)
                {
                    //render image
                    html.Append("<img src=\"" + part.value + "\"/>");
                    cancelValue = true;
                }

                if(showValue == true && cancelValue == false) { html.Append(part.value); }
                if(endTags != "") { html.Append(endTags); }
                lastPart = part;
            }

            return html.ToString();
        }
    }
}
