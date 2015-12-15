using System.Collections.Generic;
using System.IO;
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
            Scaffold scaffold;
            Scaffold scaffold2;
            SqlClasses.Dashboard sqlDash;
            SqlReader reader;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/articles/create\" class=\"button blue\">New Article</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            //setup scaffolding variables
            scaffold = new Scaffold(S, "/app/includes/dashboard/articles/list.html", "", new string[] { "content" });

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
                                if(S.Request.Form["url"] != "")
                                {
                                    //analyze an article ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                    var serveArticles = new Services.Dashboard.Articles(S, S.Page.Url.paths);
                                    var url = S.Request.Form["Url"];
                                    var article = serveArticles.Analyze(url);

                                    //setup article analysis results
                                    scaffold2 = new Scaffold(S, "/app/includes/dashboard/articles/analyzed.html", "", new string[] { "name", "id" });

                                    scaffold2.Data["article-title"] = article.pageTitle;
                                    scaffold2.Data["article-url"] = article.url;

                                    //create rendered web page ///////////////////////////////////////////////////
                                    scaffold2.Data["rendered-url"] = article.url;

                                    //create raw html Ace Editor ///////////////////////////////////////////////////
                                    scaffold2.Data["raw-html"] = article.rawHtml.Replace("<","&lt;").Replace(">","&gt;");

                                    //create dom structure ///////////////////////////////////////////////////////
                                    var domStructure = new List<string>();
                                    var i = -1;
                                    foreach(DomElement tag in article.elements)
                                    {
                                        i++;
                                        domStructure.Add("<div class=\"tag\"><div class=\"line-num\">" + i + "</div>" + string.Join(" > ", tag.hierarchyTags) + (tag.isClosing == false && tag.isSelfClosing == true ? " > " + tag.tagName : "") + "</div>");
                                    }
                                    scaffold2.Data["dom-structure"] = string.Join("\n", domStructure.ToArray());

                                    //create tag names ///////////////////////////////////////////////////////
                                    var tagNames = new List<string>();
                                    i = 0;
                                    foreach (var tagName in article.tagNames)
                                    {
                                        i++;
                                        tagNames.Add("<div class=\"tag-name\">" + tagName.name + "<div class=\"tag-info\">(" + tagName.count + ")</div></div>");
                                    }
                                    scaffold2.Data["tag-names"] = string.Join("\n", tagNames.ToArray());

                                    //create sorted text ///////////////////////////////////////////////////////
                                    var textSorted = new List<string>();
                                    foreach (var text in article.tags.text)
                                    {
                                        var tag = article.elements[text.index];
                                        //get info about text
                                        var info = 
                                            "<div class=\"info\">" + 
                                                "<div class=\"label\">Type:</div><div class=\"data\">" + text.type + "</div>" +
                                            "</div>" +
                                            "<div class=\"info\">" +
                                                "<div class=\"label\">DOM:</div><div class=\"data\">" + string.Join(" > ", article.elements[text.index].hierarchyTags) + "</div>" +
                                            "</div>";

                                        textSorted.Add(
                                            "<div class=\"text\">" + 
                                                "<div class=\"line-num\">" + text.index + "</div>" + 
                                                "<div class=\"raw\" onclick=\"$(this).find('.text-info').toggleClass('expanded')\">" + tag.text.Replace("<","&lt;").Replace(">","&gt;") +
                                                    "<div class=\"text-info \"><div class=\"contents\">" + info + "</div></div>" +
                                                "</div>" + 
                                            "</div>");
                                    }
                                    scaffold2.Data["text-sorted"] = string.Join("\n", textSorted.ToArray());

                                    //create sorted words ///////////////////////////////////////////////////////
                                    var wordsSorted = new List<string>();
                                    foreach (var word in article.words)
                                    {
                                        var info = "";
                                        //get info about text
                                        info += "<div class=\"info\"><div class=\"label\">Type:</div><div class=\"data\">" + word.type + "</div></div>";

                                        wordsSorted.Add(
                                            "<div class=\"word\" onclick=\"$(this).find('.word-info').toggleClass('expanded')\">" + 
                                                word.word + "<div class=\"word-info\"><div class=\"contents\">" + info + "</div></div>" +
                                            "</div>");
                                    }
                                    scaffold2.Data["words-sorted"] = string.Join("\n", wordsSorted.ToArray());

                                    //create article ///////////////////////////////////////////////////////
                                    var body = new List<string>();
                                    foreach (var bod in article.body)
                                    {
                                        body.Add(article.elements[bod].text);
                                        
                                    }
                                    scaffold2.Data["article"] = "<span>" + string.Join("</span><span>", body.ToArray()) + "</span>";

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
                                    scaffold2.Data["anchorlinks"] = "<div class=\"link\">" + string.Join("</div><div class=\"link\">", anchorLinks.ToArray()) + "</div>";

                                    //render article analysis results ////////////////////////////////////////////////////////////////////////////////////////////////////
                                    scaffold.Data["content"] = scaffold2.Render();

                                    //load javascript file
                                    scriptFiles += "<script type=\"text/javascript\" src=\"/scripts/ace/ace.js\"></script>";
                                    S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/analyzed.js");
                                }


                                //create new article from posted form
                                //sqlDash = new SqlClasses.Dashboard(S);
                                //int articleId = sqlDash.AddArticle(S.Request.Form["title"], S.Request.Form["description"], int.Parse(S.Request.Form["category"]));
                                //scaffold2 = new Scaffold(S, "/app/includes/dashboard/articles/created.html", "", new string[] { "name", "id" });
                                //scaffold2.Data["name"] = S.Request.Form["title"];
                                //scaffold2.Data["id"] = articleId.ToString();
                                //scaffold.Data["content"] = scaffold2.Render();
                                break;
                            }
                        }
                        //load article creation form
                        scaffold2 = new Scaffold(S, "/app/includes/dashboard/articles/create.html", "", new string[] { "categories"});

                        //render form
                        scaffold.Data["content"] = scaffold2.Render();
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
                                scaffold2 = new Scaffold(S, "/app/includes/dashboard/articles/edit.html", "", new string[] { "name", "content", "id" });
                                scaffold2.Data["name"] = reader.Get("title");
                                scaffold2.Data["id"] = reader.Get("articleid");
                                if (File.Exists(S.Server.MapPath("/content/articles/" + reader.Get("articleid") + ".html")))
                                {
                                    scaffold2.Data["content"] = S.Server.OpenFile("/content/articles/" + reader.Get("articleid") + ".html").Replace("\"","\\\"").Replace("\n","\\n");
                                }
                                scaffold.Data["content"] = scaffold2.Render();
                                S.Page.RegisterJSFromFile("/app/includes/dashboard/articles/edit.js");
                            }
                        }
                        
                        break;
                }
            }
            else
            {
                //get article list from web service
                Services.Dashboard.Articles ws = new Services.Dashboard.Articles(S, S.Page.Url.paths);
                //scaffold.Data["content"] = ws.GetArticles();
            }


            return scaffold.Render();
        }
    }
}
