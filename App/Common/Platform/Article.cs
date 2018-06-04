using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Utility.Strings;
using Utility.Serialization;
using Utility.DOM;
using Collector.Models.Article;
using Collector.Common.Analyze;

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

        public static AnalyzedArticle Download(string url)
        {
            var path = Server.MapPath(Server.Instance.Cache["browserPath"].ToString());

            //execute WebBrowser console app to get DOM results from offscreen Chrome browser
            var obj = Utility.Shell.Execute(path, "-url " + url, path.Replace("WebBrowser.exe",""), 60);
            //deserialize and process results
            return Html.DeserializeArticle(obj);
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
            var body = new StringBuilder();
            var containerType = "";
            var containerChangedType = "";
            var oldType = "";
            var containerIndex = 0;
            var tagIndex = 0;
            var tagId = 0;
            var tagChanged = false;
            var tagTypes = new string[] { "p", "h1", "h2", "h3", "h4", "h5", "h6" };
            var containTypes = new string[] { "a" };
            var endTags = "";
            var startTags = "";
            var hasSpace = false;
            DomElement elemTag;
            DomElement elem;
            DomElement specialTag;
            AnalyzedWord aword;
            List<AnalyzedWord> awords;
            List<string> hierarchyTags;
            int sentenceIndex = 1;
            var sentences = new List<string>();
            var sentenceEnding = "";
            double sentenceScore = 0;
            int sentenceCount = 0;
            var parWords = "";
            var paragraph = "";
            var paragraphs = 0;
            var parType = 0;
            var parTypeName = "";
            var paraEnd = "";
            var wordClasses = "";
            var sentenceClasses = "";
            var paragraphHtml = "";
            var str1 = "";
            var scoreCount = 5;
            string[] wordlist;
            int bod;
                        
            for (var x = 0; x < article.body.Count; x++)
            {
                bod = article.body[x];
                tagChanged = false;
                elem = article.elements[bod];
                containerChangedType = containerType;
                hierarchyTags = elem.HierarchyTags();

                //check for container (p, h1, h2, h3, etc)
                foreach (string tag in tagTypes)
                {
                    tagIndex = hierarchyTags.ToList().LastIndexOf(tag);
                    if (tagIndex < 0) { continue; }
                    tagIndex = elem.hierarchyIndexes[tagIndex];
                    if (tagIndex >= 0)
                    {
                        elemTag = article.elements[tagIndex];

                        if (hierarchyTags.Contains(tag))
                        {
                            if (containerType == tag)
                            {
                                if (containerIndex != elemTag.index)
                                {
                                    //different tag
                                    tagChanged = true;
                                }
                            }
                            else
                            {
                                //current tag is not same
                                containerType = tag;
                                tagChanged = true;
                            }
                        }
                        if (tagChanged == true)
                        {
                            containerIndex = elemTag.index;
                            break;
                        }
                    }
                }
                if (tagChanged == false && containerChangedType != "" && hierarchyTags.Contains(containerChangedType) == false)
                {
                    //old tag doesn't exist in hierarchy
                    tagChanged = true;
                    containerType = "";
                    containerIndex = -1;
                }

                //create tags based on element tag hierarchy
                foreach (string tag in containTypes)
                {
                    tagIndex = hierarchyTags.ToList().LastIndexOf(tag);
                    if (tagIndex >= 0)
                    {
                        //found special tag in hierarchy
                        tagId = elem.hierarchyIndexes[tagIndex];
                        specialTag = article.elements[tagId];
                        var attrs = "";
                        var useTag = false;
                        switch (tag)
                        {
                            case "a":
                                str1 = "#";
                                if (specialTag.attribute.ContainsKey("href"))
                                {
                                    str1 = specialTag.attribute["href"];
                                }
                                attrs = " href=\"/dashboard/articles?url=" + WebUtility.UrlEncode(str1) + "\" target=\"_blank\"";
                                useTag = true;
                                break;
                        }
                        if(useTag == true)
                        {
                            startTags += "<" + tag + attrs + ">";
                            endTags = "</" + tag + "> " + endTags;
                        }
                    }
                }


                if (tagChanged == true)
                {
                    if (oldType != "") { body.Append("</" + oldType + ">"); }
                    if (containerType != "") { body.Append("<" + containerType + ">"); }
                    oldType = containerType;
                }


                if (x < article.body.Count - 1)
                {
                    //check for adding an extra space after this tag (for grammar)
                    switch (article.elements[article.body[x + 1]].text.Substring(0, 1))
                    {
                        case ".": case ":": case ";": case ",":
                        case "!": case "?": case "\"": case "'":
                        case "/": case "\\":
                            break;
                        default:
                            endTags = " " + endTags;
                            break;
                    }
                }


                //separate paragraph into sentences
                sentences = Html.GetSentences(article.elements[bod].text);
                sentenceCount += sentences.Count();
                paragraph = "";
                paragraphs+=1;


                foreach (var s in sentences)
                {
                    //detect whether or not the sentence is important
                    //TODO: figure out paragraph type
                    parType = 1;
                    switch (parType)
                    {
                        case 1:
                            parTypeName = "normal";
                            break;

                        case 2:
                            parTypeName = "important";
                            break;

                        case 3:
                            parTypeName = "title";
                            break;

                        case 4:
                            parTypeName = "published";
                            break;

                        case 5:
                            parTypeName = "unknown";
                            break;

                        default:
                            parTypeName = "normal";
                            break;

                    }

                    //check to see if this is the end of the sentence
                    wordlist = Html.GetWordsFromText(s, Rules.separatorExceptions);
                    if(wordlist.Length > 0)
                    {
                        sentenceEnding = wordlist[wordlist.Length - 1];
                        if (tagChanged == true)
                        {
                            //tag changed, this is the end of a sentence
                            sentenceIndex += 1;
                            sentenceClasses = "";
                        }

                        //find sentence score based on word list importance
                        sentenceScore = 0;
                        if (wordlist.Length > 5)
                        {
                            foreach (var w in wordlist)
                            {
                                //find analyzed word
                                awords = article.words.Where(wr => wr.word == w.ToLower()).ToList();
                                if (awords.Count == 1)
                                {
                                    aword = awords[0];
                                    if (aword.importance >= 5)
                                    {
                                        if (aword.id > 0)
                                        {
                                            //word is stored in the database
                                            if (aword.importance >= 10)
                                            {
                                                //word is a name
                                                sentenceScore += (scoreCount * 0.1);
                                            }
                                            else
                                            {
                                                //word is potentially important
                                                sentenceScore += (scoreCount * 0.149);
                                            }

                                        }
                                        else
                                        {
                                            //word is potentially important
                                            sentenceScore += (scoreCount * 0.149);
                                        }

                                    }
                                    if (Rules.badWords.Contains(aword.word))
                                    {
                                        //punish score if negative words are found (shit, bitch, fuck)
                                        sentenceScore -= (scoreCount * 0.5);
                                    }
                                }
                            }

                            if (sentenceScore > 0)
                            {
                                if (sentenceScore > scoreCount) { sentenceScore = scoreCount; }
                                sentenceScore = Math.Round(sentenceScore);
                                //sentenceScore = 0.1 * sentenceScore;
                                //sentenceScore = Math.Round(5.0 / (wordlist.Length / 4.0) * sentenceScore);
                                if (sentenceScore > 0)
                                {
                                    sentenceClasses += " score" + Math.Round(sentenceScore);
                                }
                            }
                        }

                        //separate each word into a span tag
                        parWords = "";
                        var a = "";
                        for (var u = 0; u < wordlist.Length; u++)
                        {
                            a = wordlist[u];
                            wordClasses = "";
                            if (a.Length == 1)
                            {
                                if (Html.isSentenceSeparator(a, Rules.separatorExceptions))
                                {
                                    wordClasses += " separator";
                                }
                            }
                            hasSpace = false;
                            if (u < wordlist.Length - 1)
                            {
                                //check for adding an extra space after this tag (for grammar)
                                switch (wordlist[u + 1].Substring(0, 1))
                                {
                                    case ".": case ":": case ";": case ",": case "!":
                                    case "?": case "'": case "/": case "\\":
                                    case ")": case "]": case "}":
                                        break;
                                    default:
                                        hasSpace = true;
                                        break;
                                }
                            }
                            else
                            {
                                hasSpace = true;
                            }
                            switch(a.Substring(a.Length - 1))
                            {
                                case "(": case "[": case "{":
                                    hasSpace = false;
                                    break;
                            }

                            parWords += "<span class=\"word sentence" + sentenceIndex + " " + parTypeName + wordClasses + sentenceClasses + (hasSpace ? " space" : "") + "\">" + a + (hasSpace ? " " : "") + "</span>\n";
                        }
                        paragraph += parWords;

                        //check for end of sentence
                        switch (sentenceEnding)
                        {
                            case ".": case "?": case "!":
                                //last word is a sentence ending marker
                                sentenceIndex += 1;
                                break;
                        }
                                        
                    }
                }


                //TODO: figure out if current body element is the end of a paragraph
                paragraphHtml = startTags + paragraph + paraEnd + endTags;
                if (paragraphHtml.Trim() != "")
                {
                    body.Append(startTags + paragraph + paraEnd + endTags);
                }
                endTags = "";
                startTags = "";
                paraEnd = "";
                paragraph = "";
            }
            if (containerType != "")
            {
                body.Append("</" + containerType + ">\n");
            }
            return body.ToString();
        }
    }
}
