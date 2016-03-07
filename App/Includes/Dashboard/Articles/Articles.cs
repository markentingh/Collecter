using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Collector.Utility.DOM;
using Collector.Services;


namespace Collector.Includes
{
    public class Articles : Include
    {
        private string[] separatorExceptions = new string[]{"'", "\""};

        public Articles(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/articles/create\" class=\"button blue\">Analyze Article</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            //determine which section to load for articles
            if (S.Page.Url.paths.Length > 2)
            {
                switch (S.Page.Url.paths[2].ToLower())
                {
                    case "create":
                    
                        if (S.Request.ContentType != null)
                        { 
                            if (S.Request.Form.Count > 0)
                            {
                                if (S.Request.Form["url"] != "")
                                {
                                    scaffold = LoadAnalyzedArticle(S.Request.Form["url"]);
                                }
                            }
                        } 
                        if(scaffold == null) { scaffold = LoadCreateArticleForm(); }

                        //render form
                        parentScaffold.Data["menu"] = "";
                        break;

                    case "analyze":
                        if (S.Request.Query.ContainsKey("url"))
                        {
                            scaffold = LoadAnalyzedArticle(S.Request.Query["url"]);
                        }
                        break;
                }
            }
            else
            {
                if (S.Request.ContentType != null)
                {
                    if (S.Request.Form.Count > 0)
                    {
                        if (S.Request.Form["url"] != "")
                        {
                            //analyze article
                            scaffold = LoadAnalyzedArticle(S.Request.Form["url"]);
                        }
                    }
                }
                if (scaffold == null)
                {
                    //get article list from web service
                    scaffold = LoadArticleList();
                }
                
            }


            return scaffold.Render();
        }

        private Scaffold LoadArticleList()
        {
            var scaffold = new Scaffold(S, "/app/includes/dashboard/articles/list.html", "", new string[] { "content" });
            Services.Articles articles = new Services.Articles(S, S.Page.Url.paths);
            scaffold.Data["content"] = string.Join("\n", articles.GetArticlesUI().html);
            S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/list.js");
            return scaffold;
        }

        private Scaffold LoadCreateArticleForm()
        {
            //load article creation form
            return new Scaffold(S, "/app/includes/dashboard/articles/create.html", "", new string[] { "categories" });
        }

        private Scaffold LoadAnalyzedArticle(string url) {
            Scaffold scaffold = new Scaffold(S, "/app/includes/dashboard/articles/analyzed.html", "", new string[] { "name", "id" });
            if (url != "")
            {
                //analyze an article ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var serveArticles = new Services.Articles(S, S.Page.Url.paths);
                var article = serveArticles.Analyze(url);

                //check if article is empty
                if(article.title=="" || article.body.Count == 0) { return scaffold; }

                //create article ///////////////////////////////////////////////////////
                var body = new List<string>();
                var containerType = "";
                var containerChangedType = "";
                var oldType = "";
                var containerIndex = 0;
                var tagIndex = 0;
                var tagId = 0;
                var tagChanged = false;
                var tagTypes = new string[] { "p", "h1", "h2", "h3", "h4", "h5", "h6" };
                var containTypes = new string[] { "a" };
                var negativeWords = new string[] {
                    "shit", "crap", "asshole", "shitty", "bitch", "slut", "whore",
                    "fuck", "fucking", "fucker", "fucked", "fuckers", "fucks"
                };
                var endTags = "";
                var startTags = "";
                DomElement elemTag;
                DomElement elem;
                DomElement specialTag;
                Services.Articles.AnalyzedWord aword;
                List<Services.Articles.AnalyzedWord> awords;
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

                    foreach (string tag in tagTypes)
                    {
                        tagIndex = elem.hierarchyTags.ToList().LastIndexOf(tag);
                        if (tagIndex < 0) { continue; }
                        tagIndex = elem.hierarchyIndexes[tagIndex];
                        if (tagIndex >= 0)
                        {
                            elemTag = article.elements[tagIndex];

                            if (elem.hierarchyTags.Contains(tag))
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
                    if (tagChanged == false && containerChangedType != "" && elem.hierarchyTags.Contains(containerChangedType) == false)
                    {
                        //old tag doesn't exist in hierarchy
                        tagChanged = true;
                        containerType = "";
                        containerIndex = -1;
                    }

                    //create tags based on element tag hierarchy
                    foreach (string tag in containTypes)
                    {
                        tagIndex = elem.hierarchyTags.ToList().LastIndexOf(tag);
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
                                    attrs = " href=\"/dashboard/articles?url=" + S.Util.Str.UrlEncode(str1) + "\" target=\"_blank\"";
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
                        if (oldType != "") { body.Add("</" + oldType + ">"); }
                        if (containerType != "") { body.Add("<" + containerType + ">"); }
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
                    sentences = serveArticles.GetSentences(article.elements[bod].text);
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
                        wordlist = serveArticles.GetWords(s, separatorExceptions);
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
                                        if (negativeWords.Contains(aword.word))
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
                            foreach (var w in wordlist)
                            {
                                wordClasses = "";
                                if (w.Length == 1)
                                {
                                    if (serveArticles.isSentenceSeparator(w, separatorExceptions))
                                    {
                                        wordClasses += " separator";
                                    }
                                }

                                        
                                parWords += "<span class=\"word sentence" + sentenceIndex + " " + parTypeName + wordClasses + sentenceClasses + "\">" + w + "</span>\n";
                            }
                            paragraph += parWords;

                            //check for end of sentence
                            switch (sentenceEnding)
                            {
                                case ".": case ":": case ")": case "}":
                                case "]": case "\"": case "?": case "!":
                                    //last word is a sentence ending marker
                                    sentenceIndex += 1;
                                    break;
                            }
                                        
                        }
                    }
                    if(1 == 0) { paraEnd = "<div class=\"para-end\"></div>"; }


                    //TODO: figure out if current body element is the end of a paragraph
                    paragraphHtml = startTags + paragraph + paraEnd + endTags;
                    if (paragraphHtml.Trim() != "")
                    {
                        body.Add(startTags + paragraph + paraEnd + endTags);
                    }
                    endTags = "";
                    startTags = "";
                    paraEnd = "";
                    paragraph = "";
                }
                if (containerType != "")
                {
                    body.Add("</" + containerType + ">\n");
                }
                article.totalParagraphs = paragraphs;
                article.totalSentences = sentenceCount;
                scaffold.Data["article"] = string.Join("", body.ToArray());

                //create rendered web page ///////////////////////////////////////////////////
                //scaffold.Data["rendered-url"] = article.url;

                //create raw html Ace Editor ///////////////////////////////////////////////////
                scaffold.Data["raw-html"] = article.rawHtml.Replace("<", "&lt;").Replace(">", "&gt;");

                //create dom structure ///////////////////////////////////////////////////////
                var domStructure = new List<string>();
                var i = -1;
                foreach (DomElement tag in article.elements)
                {
                    i++;
                    domStructure.Add("<div class=\"tag\"><div class=\"line-num\">" + (i+1) + "</div>" + string.Join(" > ", tag.hierarchyTags) + (tag.isClosing == false && tag.isSelfClosing == true ? " > " + tag.tagName : "") + "</div>");
                }
                scaffold.Data["dom-structure"] = string.Join("\n", domStructure.ToArray());
                scaffold.Data["dom-count"] = String.Format("{0:N0}", i + 1);
                //create tag names ///////////////////////////////////////////////////////
                var tagNames = new List<string>();
                i = 0;
                foreach (var tagName in article.tagNames)
                {
                    if(tagName.name.Substring(0,1) == "/") { continue; }
                    tagNames.Add("<div class=\"tag-name\">" + tagName.name + "<div class=\"tag-info\">(" + tagName.count + ")</div></div>");
                }
                scaffold.Data["tag-names"] = string.Join("\n", tagNames.ToArray());
                scaffold.Data["tag-names-count"] = String.Format("{0:N0}", tagNames.Count());
                //create sorted words ///////////////////////////////////////////////////////
                var wordsSorted = new List<string>();
                var commonWords = serveArticles.GetCommonWords();
                var wordType = "";
                var totalWords = 0;
                i = 0;
                foreach (var word in article.words)
                {

                    wordType = serveArticles.GetWordTypeClassNames(article, word, commonWords);
                    totalWords += word.count;

                    wordsSorted.Add(
                        "<div class=\"word" + wordType + "\">" +
                            "<div class=\"word-info\">" + word.count + "</div>" +
                            "<div class=\"value\">" + (word.importance == 10 ? S.Util.Str.Capitalize(word.word) : S.Util.Str.HtmlEncode(word.word)) + "</div>" +
                        "</div>");
                }
                scaffold.Data["word-count"] = String.Format("{0:N0}", totalWords) + " total / " + String.Format("{0:N0}", article.words.Count) + " unique";
                scaffold.Data["words-sorted"] = string.Join("\n", wordsSorted.ToArray());

                //create subjects ///////////////////////////////////////////////////////
                var subjects = new List<string>();
                var subjectId = 0;
                var subjectTitle = "";
                var subjectScore = 0;
                var sub = "";
                var hier = "";
                var subdetails = "";
                var breadcrumb = "";
                var hierarchy = "";
                Services.Articles.ArticleSubject subj;
                i = 0;
                foreach (var subject in article.subjects)
                {
                    i++;
                    if (subject.breadcrumb != null)
                    {
                        sub = string.Join(" > ", subject.breadcrumb) + " > " + subject.title;
                        hier = string.Join(" > ", subject.hierarchy) + " > " + subject.id;
                    }
                    else
                    {
                        sub = subject.title;
                        hier = subject.id.ToString();
                    }
                    if(i == 1)
                    {
                        scaffold.Data["subject-breadcrumb"] = sub;
                        breadcrumb = sub;
                        hierarchy = hier;
                        subjectId = subject.id;
                        subjectTitle = subject.title;
                        subjectScore = subject.score;
                    }
                    subdetails = "<span class=\"number\">" + subject.count + "</span>\n";
                    if(subject.parentIndexes.Count > 0)
                    {
                        for (var x = 0; x < subject.parentIndexes.Count; x++)
                        {
                            subj = article.subjects.Where(s => s.id == subject.parentIndexes[x]).ToList()[0];
                            subdetails += "<span class=\"addition\">+</span><span class=\"number\">" + subj.count + "</span>\n";
                        }
                    }
                    if(subject.breadcrumb.Length >= 1)
                    {
                        //multiply
                        subdetails = "<span class=\"symbol\">(</span>" + subdetails + "<span class=\"symbol\">)</span>" +
                            "<span class=\"multiply\">*</span>" + "<span class=\"number\">" + (subject.breadcrumb.Length + 1) + "</span>\n";
                    }
                    subdetails += "<span class=\"number\"> = <b>" + String.Format("{0:N0}", subject.score) + "</b></span>\n";
                    subjects.Add(
                        "<div class=\"subject\"><span class=\"score\" title=\"Score\"><div class=\"details\">" + 
                        subdetails + "</div>" + String.Format("{0:N0}", subject.score) + "</span><span class=\"word-count\" title=\"Word count\">" +
                        String.Format("{0:N0}", subject.count)  + "</span><span class=\"subject-breadcrumb\">" + S.Util.Str.Capitalize(sub) + "</span></div>");
                }
                scaffold.Data["subjects"] = string.Join("\n", subjects.ToArray());

                //create phrases ///////////////////////////////////////////////////////
                var phrases = new List<string>();
                i = 0;
                foreach (var phrase in article.phrases)
                {
                    phrases.Add("<div class=\"phrase\"><div class=\"value\">" + phrase.phrase + "</div></div>");
                }
                scaffold.Data["phrase-count"] = article.phrases.Count().ToString();
                scaffold.Data["phrases"] = string.Join("\n", phrases.ToArray());

                //create anchor links ///////////////////////////////////////////////////////
                var anchorLinks = new List<string>();
                foreach (var anchor in article.tags.anchorLinks)
                {
                    var tag = article.elements[anchor];
                    if (tag.attribute.ContainsKey("href"))
                    {
                        anchorLinks.Add("<a href=\"" + tag.attribute["href"] + "\" target=\"_blank\">" + tag.attribute["href"] + "</a>");
                    }
                }
                scaffold.Data["anchorlinks"] = "<div class=\"link\">" + string.Join("</div><div class=\"link\">", anchorLinks.ToArray()) + "</div>";

                //load bug reports ///////////////////////////////////////////////////
                var bugs = serveArticles.GetBugReports(article);
                scaffold.Data["bug-count"] = bugs[0];
                scaffold.Data["bugs"] = bugs[1];

                //load article listing ////////////////////////////////////////////////
                var fileSize = (article.rawHtml.Length / 1024.0);
                scaffold.Data["listing"] =
                    serveArticles.GetArticleListItem(
                        article.title, article.url, breadcrumb, hierarchy, subjectId, subjectTitle, subjectScore, fileSize, article.totalWords,
                        article.totalSentences, article.totalImportantWords, string.Join(", ", article.years), article.totalBugsOpen, article.totalBugsResolved 
                        );

                //render article analysis results ////////////////////////////////////////////////////////////////////////////////////////////////////
                scaffold.Data["content"] = scaffold.Render();

                //finally, save article to database
                serveArticles.SaveArticle(article);
                //TODO: save article object to file

                //load javascript file
                scriptFiles += "<script type=\"text/javascript\" src=\"/scripts/ace/ace.js\"></script>";
                S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/analyzed.js");
                S.Page.RegisterJS("articleid", "S.analyzed.articleId=" + article.id);
            }


            //create new article from posted form
            //sqlDash = new SqlClasses.Dashboard(S);
            //int articleId = sqlDash.AddArticle(S.Request.Form["title"], S.Request.Form["description"], int.Parse(S.Request.Form["category"]));
            //scaffold = new Scaffold(S, "/app/includes/dashboard/articles/created.html", "", new string[] { "name", "id" });
            //scaffold.Data["name"] = S.Request.Form["title"];
            //scaffold.Data["id"] = articleId.ToString();
            //scaffold.Data["content"] = scaffold.Render();
            return scaffold;
        }

        
    }
}
