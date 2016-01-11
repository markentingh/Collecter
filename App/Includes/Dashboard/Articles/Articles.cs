using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Collector.Utility.DOM;


namespace Collector.Includes.Dashboard
{
    public class Articles : Include
    {
        public Articles(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold)
        {

        }

        public override string Render()
        {
            Scaffold scaffold = null;
            SqlClasses.Dashboard sqlDash;
            SqlReader reader;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/articles/create\" class=\"button blue\">New Article</a></li></ul></div>";
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
                                    scaffold = LoadAnalyzedArticle();
                                }
                            }
                        }
                        if(scaffold == null) { scaffold = LoadCreateArticleForm(); }

                        //render form
                        parentScaffold.Data["menu"] = "";
                        break;

                    case "edit":
                        //load article editor
                        if(S.Page.Url.paths.Length > 3)
                        {
                            
                            //get article from sql
                            sqlDash = new SqlClasses.Dashboard(S);
                            reader = sqlDash.GetArticle(int.Parse(S.Page.Url.paths[3]));
                            if(reader.Rows.Count > 0)
                            {
                                reader.Read();
                                scaffold = new Scaffold(S, "/app/includes/dashboard/articles/edit.html", "", new string[] { "name", "content", "id" });
                                scaffold.Data["name"] = reader.Get("title");
                                scaffold.Data["id"] = reader.Get("articleid");
                                if (File.Exists(S.Server.MapPath("/content/articles/" + reader.Get("articleid") + ".html")))
                                {
                                    scaffold.Data["content"] = S.Server.OpenFile("/content/articles/" + reader.Get("articleid") + ".html").Replace("\"","\\\"").Replace("\n","\\n");
                                }
                                S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/edit.js");
                            }
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
                            scaffold = LoadAnalyzedArticle();
                        }
                    }
                }
                if (scaffold == null)
                {
                    //get article list from web service
                    scaffold = new Scaffold(S, "/app/includes/dashboard/articles/list.html", "", new string[] { "content" });
                    Services.Dashboard.Articles articles = new Services.Dashboard.Articles(S, S.Page.Url.paths);
                    reader = articles.GetArticles(1, 50);
                    if (reader.Rows.Count > 0)
                    {
                        var html = new List<string>();
                        while (reader.Read())
                        {
                            html.Add("<div class=\"article\"><a href=\"" + reader.Get("url") + "\" onclick=\"S.articles.analyzeArticle('" + reader.Get("url") + "'); return false\">" + reader.Get("title") + "</a><div class=\"desc\">" + reader.Get("summary") + "</div></div>");
                        }
                        scaffold.Data["content"] = string.Join("\n", html);
                        S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/list.js");
                    }
                }
                
            }


            return scaffold.Render();
        }

        private Scaffold LoadCreateArticleForm()
        {
            //load article creation form
            return new Scaffold(S, "/app/includes/dashboard/articles/create.html", "", new string[] { "categories" });
        }

        private Scaffold LoadAnalyzedArticle() {
            Scaffold scaffold = new Scaffold(S, "/app/includes/dashboard/articles/analyzed.html", "", new string[] { "name", "id" });
            if (S.Request.ContentType != null)
            {
                if (S.Request.Form.Count > 0)
                {
                    if (S.Request.Form["url"] != "")
                    {
                        //analyze an article ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        var serveArticles = new Services.Dashboard.Articles(S, S.Page.Url.paths);
                        var url = S.Request.Form["Url"];
                        var info = "";
                        var article = serveArticles.Analyze(url);

                        //save article to database
                        serveArticles.SaveArticle(article);
                        //TODO: save article object to file

                        //setup article analysis results
                        scaffold.Data["article-title"] = article.pageTitle;
                        scaffold.Data["article-url"] = article.url;

                        //create article ///////////////////////////////////////////////////////
                        var body = new List<string>();
                        var containerType = "";
                        var oldType = "";
                        var containerIndex = 0;
                        var tagIndex = 0;
                        DomElement elemTag;
                        var tagChanged = false;
                        var tagTypes = new string[] { "p", "h1", "h2", "h3", "h4", "h5", "h6" };
                        var containTypes = new string[] { "a" };
                        var endTags = "";
                        var startTags = "";
                        DomElement elem;
                        int bod;
                        for (var x = 0; x < article.body.Count; x++)
                        {
                            bod = article.body[x];
                            tagChanged = false;
                            elem = article.elements[bod];

                            foreach (string tag in tagTypes)
                            {
                                tagIndex = elem.hierarchyTags.ToList().IndexOf(tag);
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

                            foreach (string tag in containTypes)
                            {
                                tagIndex = elem.hierarchyTags.ToList().IndexOf(tag);
                                if (tagIndex >= 0)
                                {
                                    var attrs = "";
                                    switch (tag)
                                    {
                                        case "a":
                                            attrs = " href=\"#\"";
                                            break;

                                    }
                                    startTags += "<" + tag + attrs + ">";
                                    endTags += "</a>";
                                }
                            }


                            if (tagChanged == true)
                            {
                                body.Add("</" + oldType + ">");

                                body.Add("<" + containerType + ">");
                                oldType = containerType;
                            }

                            if (x < article.body.Count - 1)
                            {
                                //check for adding an extra space after this tag (for grammar)
                                switch (article.elements[article.body[x + 1]].text.Substring(0, 1))
                                {
                                    case ".":
                                    case ":":
                                    case ";":
                                    case ",":
                                    case "!":
                                    case "?":
                                    case "\"":
                                    case "'":
                                    case "/":
                                    case "\\":
                                        break;
                                    default:
                                        endTags = " " + endTags;
                                        break;
                                }
                            }


                            body.Add(startTags + article.elements[bod].text + endTags);
                            endTags = "";
                            startTags = "";

                        }
                        if (containerType != "")
                        {
                            body.Add("</" + containerType + ">");
                        }
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
                            domStructure.Add("<div class=\"tag\"><div class=\"line-num\">" + i + "</div>" + string.Join(" > ", tag.hierarchyTags) + (tag.isClosing == false && tag.isSelfClosing == true ? " > " + tag.tagName : "") + "</div>");
                        }
                        scaffold.Data["dom-structure"] = string.Join("\n", domStructure.ToArray());

                        //create tag names ///////////////////////////////////////////////////////
                        var tagNames = new List<string>();
                        i = 0;
                        foreach (var tagName in article.tagNames)
                        {
                            i++;
                            tagNames.Add("<div class=\"tag-name\">" + tagName.name + "<div class=\"tag-info\">(" + tagName.count + ")</div></div>");
                        }
                        scaffold.Data["tag-names"] = string.Join("\n", tagNames.ToArray());

                        //create sentences ///////////////////////////////////////////////////////

                        i = 0;
                        var classes = "";
                        var sentences = new List<string>();
                        var se = "";
                        foreach (string sentence in article.sentences)
                        {
                            i++;
                            se = sentence.ToLower();
                            classes = "";
                            if (article.words.Where(w => w.id > 0)
                                .Where(w => 
                                se.IndexOf(w.word + " ") >= 0 || se.IndexOf(w.word + ".") >= 0 || se.IndexOf(w.word + ",") >= 0 ||
                                se.IndexOf(w.word + ":") >= 0 || se.IndexOf(w.word + ";") >= 0 || se.IndexOf(w.word + "]") >= 0 || 
                                se.IndexOf(w.word + ")") >= 0 || se.IndexOf(w.word + "\"") >= 0 || se.IndexOf(w.word + "'") >= 0
                                ).Count() > 0)
                            {
                                classes = " special";
                            }
                            sentences.Add(
                                "<div class=\"text" + classes + "\">" +
                                    "<div class=\"line-num\">" + i + "</div>" +
                                    "<div class=\"raw\" onclick=\"$(this).find('.text-info').toggleClass('expanded')\">" + sentence +
                                    "</div>" +
                                "</div>");
                        }
                        scaffold.Data["sentence-count"] = String.Format("{0:N0}", article.sentences.Count); 
                        scaffold.Data["sentences"] = string.Join("\n", sentences.ToArray());

                        //create sorted words ///////////////////////////////////////////////////////
                        var wordsSorted = new List<string>();
                        var commonWords = serveArticles.GetCommonWords();
                        var wordType = "";
                        var totalWords = 0;
                        i = 0;
                        foreach (var word in article.words)
                        {

                            wordType = "";
                            if (S.Util.Str.IsNumeric(word.word) == true)
                            {
                                var number = int.Parse(word.word);
                                var numtype = " number";
                                if (word.word.Length == 4)
                                {

                                    //potential year
                                    if (number <= DateTime.Now.Year + 100 && number >= 1)
                                    {
                                        if (number < 1600)
                                        {
                                            if (article.words[i + 1].word.ToLower() == "ad" || article.words[i + 1].word.ToLower() == "bc")
                                            {
                                                numtype = " year";
                                            }
                                        }
                                        else
                                        {
                                            numtype = " year";
                                        }
                                    }

                                }
                                wordType += numtype;
                                i++;
                            }

                            if (word.importance == 10) { wordType += " important"; }
                            if (commonWords.Contains(word.word.ToLower().Trim())) { wordType += " common"; }
                            else
                            {
                                if (word.importance == 0) { wordType += " symbols"; }
                            }
                            if (word.id > 0) { wordType += " database"; }

                            totalWords += word.count;

                            wordsSorted.Add(
                                "<div class=\"word" + wordType + "\">" +
                                    "<div class=\"word-info\">" + word.count + "</div>" +
                                    "<div class=\"value\">" + (word.importance == 10 ? S.Util.Str.Capitalize(word.word) : word.word) + "</div>" +
                                "</div>");
                        }
                        scaffold.Data["word-count"] = String.Format("{0:N0}", totalWords);
                        scaffold.Data["words-sorted"] = string.Join("\n", wordsSorted.ToArray());

                        //create subjects ///////////////////////////////////////////////////////
                        var subjects = new List<string>();
                        var sub = "";
                        i = 0;
                        foreach (var subject in article.subjects)
                        {
                            i++;
                            if (subject.breadcrumb != null)
                            {
                                sub = string.Join(" > ", subject.breadcrumb) + " > " + subject.title;
                            }
                            else
                            {
                                sub = subject.title;
                            }
                            subjects.Add("<div class=\"subject\"><h5>" + S.Util.Str.Capitalize(sub) + "</h5></div>");
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

                        //render article analysis results ////////////////////////////////////////////////////////////////////////////////////////////////////
                        scaffold.Data["content"] = scaffold.Render();

                        //load javascript file
                        scriptFiles += "<script type=\"text/javascript\" src=\"/scripts/ace/ace.js\"></script>";
                        S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/analyzed.js");
                    }


                    //create new article from posted form
                    //sqlDash = new SqlClasses.Dashboard(S);
                    //int articleId = sqlDash.AddArticle(S.Request.Form["title"], S.Request.Form["description"], int.Parse(S.Request.Form["category"]));
                    //scaffold = new Scaffold(S, "/app/includes/dashboard/articles/created.html", "", new string[] { "name", "id" });
                    //scaffold.Data["name"] = S.Request.Form["title"];
                    //scaffold.Data["id"] = articleId.ToString();
                    //scaffold.Data["content"] = scaffold.Render();
                }
            }
            return scaffold;
        }
    }
}
