using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
                                    //analyze an article
                                    var serveArticles = new Services.Dashboard.Articles(S, S.Page.Url.paths);
                                    var article = serveArticles.Analyze(S.Request.Form["Url"]);

                                    //setup article analysis results /////////////////////////////////////////////////////////////////////////////////////////////////////
                                    scaffold2 = new Scaffold(S, "/app/includes/dashboard/articles/analyzed.html", "", new string[] { "name", "id" });

                                    //create raw html Ace Editor
                                    scaffold2.Data["raw-html"] = article.rawHtml.Replace("<","&lt;").Replace(">","&gt;");

                                    //create dom structure
                                    var domStructure = new List<string>();
                                    foreach(DomElement tag in article.tags.elements)
                                    {
                                        domStructure.Add(string.Join(" > ", tag.hierarchyTags) + (tag.isClosing == false && tag.isSelfClosing == true ? " > " + tag.tagName : ""));
                                    }
                                    scaffold2.Data["dom-structure"] = "<div class=\"tag\">" + string.Join("</div><div class=\"tag\">", domStructure.ToArray()) + "</div>";

                                    //create text
                                    var textSorted = new List<string>();
                                    foreach (DomElement tag in article.tags.text)
                                    {
                                        textSorted.Add(tag.text);
                                    }
                                    scaffold2.Data["text-sorted"] = "<div class=\"text\">" + string.Join("</div><div class=\"text\">", textSorted.ToArray()) + "</div>";

                                    //create anchor links
                                    var anchorLinks = new List<string>();
                                    string href = "";
                                    foreach (DomElement tag in article.tags.anchorLinks)
                                    {
                                        if (tag.attribute.ContainsKey("href"))
                                        {
                                            anchorLinks.Add("a href=\"" + tag.attribute["href"] + "\" target=\"_blank\">" + tag.attribute["href"] + "</a>");
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
