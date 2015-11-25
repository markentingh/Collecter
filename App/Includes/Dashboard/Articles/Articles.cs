using System.IO;

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
                                S.Page.RegisterJSFromFile("articleedit", "/app/includes/dashboard/articles/edit.js");
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
