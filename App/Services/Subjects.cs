using System.Collections.Generic;
using System.Linq;

namespace Collector.Services
{
    public class Subjects : Service
    {

        public Subjects(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        #region "Subjects"
        public Inject AddSubject(string subjectList, int grammartype, string hierarchy, int score, bool loadUI, string element)
        {
            Inject response = new Inject();
            int parentId = 0;
            var subjects = subjectList.Replace(" , ", ",").Replace(", ", ",").Replace(" ,", ",").Split(',');
            var hier = new string[] { };
            var html = "";
            if (hierarchy != "")
            {
                hier = hierarchy.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>');
                var parentTitle = "";
                var parentBreadcrumb = "";
                if (hier.Length > 0)
                {
                    var parentHier = hier.ToList();
                    parentTitle = hier[hier.Length - 1];
                    parentHier.RemoveAt(parentHier.Count - 1);
                    parentBreadcrumb = string.Join(">", parentHier);
                }
                var reader = new SqlReader();
                reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetSubject @title='" + parentTitle + "', @breadcrumb='" + parentBreadcrumb + "'"));
                if (reader.Rows.Count > 0)
                {
                    reader.Read();
                    parentId = reader.GetInt("subjectid");
                    parentBreadcrumb = reader.Get("breadcrumb");
                }
            }
            foreach (string subject in subjects)
            {
                S.Sql.ExecuteNonQuery("EXEC AddSubject @parentid=" + parentId + ", @grammartype=" + grammartype + ", @score=" + score + ", @title='" + subject + "', @breadcrumb='" + string.Join(">", hier) + "'");
            }
            
            if (loadUI == true)
            {
                //create UI for selected subject's list
                if(parentId  > 0)
                {
                    html = LoadSubjectsUI(parentId, true, true);
                }
                else
                {
                    html = LoadSubjectsUI(parentId);
                }
                
            }
            response.inject = enumInjectTypes.replace;
            response.element = element;
            response.html = html;
            response.js = CompileJs();
            return response;
        }

        public SqlReader GetSubjects(string[] subject, int parentId = -1)
        {
            var reader = new SqlReader();
            reader.ReadFromSqlClient(
                S.Sql.ExecuteReader(
                    "EXEC GetSubjects @subjectIds='" + string.Join(",",subject) + 
                    (parentId > -1 ? "', @parentid=" + parentId : "'")
            ));
            return reader;
        }

        public SqlReader GetSubject(string title, string hierarchy)
        {
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetSubject @title='" + title + "', @breadcrumb='" + hierarchy + "'"));
            return reader;
        }

        public Inject MoveSubject(int subjectId, string hierarchy, string element)
        {
            Inject response = new Inject();
            var html = "";
            
            var title = (string)S.Sql.ExecuteScalar("SELECT title FROM Subjects WHERE subjectId=" + subjectId);
            if(title != "")
            {
                var parentId = 0;
                if (hierarchy.Trim() != "")
                {
                    var hier = hierarchy.ToLower().Split('>');
                    if(hier.Length > 1)
                    {
                        var bread = hier.Take(hier.Length - 1).ToArray();
                        parentId = (int)S.Sql.ExecuteScalar("EXEC GetSubject @title='" + hier[hier.Length - 1].ToLower() + "', @breadcrumb = '" + string.Join(">",bread).ToLower() + "'");
                    }
                    else
                    {
                        parentId = (int)S.Sql.ExecuteScalar("EXEC GetSubject @title='" + hierarchy.ToLower() + "', @breadcrumb = ''");
                    }
                }
                S.Sql.ExecuteNonQuery("EXEC MoveSubject @subjectId=" + subjectId + ", @newParent=" + parentId);
            }
            html = LoadSubjectsUI(subjectId, true, true);

            response.inject = enumInjectTypes.replace;
            response.element = element;
            response.html = html;
            response.js = CompileJs();
            return response;
        }

        public string LoadSubjectsUI(int parentId, bool getHierarchy = false, bool isFirst = false)
        {
            SqlReader reader = GetSubjects(new string[] { "" }, parentId);
            var ind = "";
            var indexes = new string[] { };
            if (reader.Rows.Count > 0)
            {
                var html = new List<string>();
                html.Add(
                    "<div class=\"accordion subjects\" id=\"subjects" + parentId + "\">\n" +
                        "<div class=\"title expanded\">{{breadcrumb}}</div>\n" +
                        "<div class=\"box expanded\">\n" +
                            "<div class=\"box-list\">"
                );
                var breadcrumbs = "";
                var parentcrumbs = "";
                var subjectId = 0;
                while (reader.Read())
                {
                    if (ind == "") { ind = reader.Get("hierarchy"); }
                    if (ind.Length > 0) { indexes = ind.Split('>'); }
                    if (subjectId == 0) { subjectId = reader.GetInt("subjectId"); }
                    breadcrumbs = reader.Get("breadcrumb");
                    if (S.Util.IsEmpty(breadcrumbs) == false)
                    {
                        if (breadcrumbs.Length > 0)
                        {
                            parentcrumbs = breadcrumbs;
                            breadcrumbs = breadcrumbs + ">" + reader.Get("title");
                        }
                        else
                        {
                            breadcrumbs = reader.Get("title");
                        }
                    }
                    else if (ind == "")
                    {
                        breadcrumbs = reader.Get("title");
                    }
                    html.Add("<div class=\"subject\" id=\"subject" + reader.GetInt("subjectId") + "\">\n" +
                                    "<a href=\"javascript:\" onclick=\"S.subjects.buttons.selectSubject('" + reader.GetInt("subjectId") + "', '" + parentId + "', '" + breadcrumbs + "', 333); return false\">\n" +
                                    S.Util.Str.Capitalize(reader.Get("title")) + "</a>\n" +
                               "</div>\n");
                }

                html.Add("</div>" +
                            "<div class=\"selection\">" +
                                "<div class=\"label goback\"></div>" +
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button green topics\">Topics</a></div>" +
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button blue add-from-subject icon-plus\" title=\"Add a Subject\"></a></div>" +
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button move-from-subject icon-reply\" title=\"Move subject to another parent\"></a></div>" +
                                (reader.GetBool("haswords") == true ? "" :
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button green calc-related-words icon-reload\" title=\"Calculate Related Words for this Subject\"></a></div>") +
                            "</div>\n" +
                            "<div class=\"option-box\"></div>\n" +
                        "</div>\n" +
                    "</div>\n" +
                    "<div class=\"topics-box\"id=\"topicsfor" + parentId + "\"><div class=\"arrow\"></div>" +
                            "<div class=\"row title\">" +
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button blue add-topic icon-plus\" title=\"Add a Topic\"></a></div>" +
                                "<h5 class=\"topic-title\"></h5>" +
                            "</div>" +
                            "<div class=\"topics-list\"></div></div>\n"
                            );

                html[0] = html[0].Replace("{{breadcrumb}}", S.Util.Str.Capitalize(parentcrumbs.Replace(">", " &gt; ")));

                var parent = "";
                if (getHierarchy == true)
                {
                    if (indexes.Length >= 1)
                    {
                        var reader2 = new SqlReader();
                        reader2.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetSubjectById @subjectId=" + parentId));
                        if (reader2.Rows.Count > 0)
                        {
                            reader2.Read();
                            var hier = reader2.Get("hierarchy");
                            var bread = reader2.Get("breadcrumb");
                            if (bread != "") { bread += ">" + reader2.Get("title"); } else { bread = reader2.Get("title"); }
                            var pId = "0";
                            if (hier != "")
                            {
                                var hier2 = hier.Split('>');
                                pId = hier2[hier2.Length - 1];
                            }
                            S.Page.RegisterJS("subj" + parentId, "S.subjects.buttons.selectSubject(" + parentId + "," + pId + ",'" + bread + "', 0, true);");
                        }
                        if (indexes.Length >= 2)
                        {
                            parent = LoadSubjectsUI(int.Parse(indexes[indexes.Length - 2]), true);
                        }
                        else if (indexes.Length == 1)
                        {
                            parent = LoadSubjectsUI(0);
                        }
                    }
                }
                return parent + "\n" + string.Join("\n", html);
            }
            return "";
        }

        #endregion

        #region "Topics (on Subjects page)"
        public Inject GetTopics(string element, int subjectId, int start = 1, string search = "", int orderby = 1)
        {
            Inject response = new Inject();
            var html = "";
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetTopics @subjectIds='" + subjectId + "', @start=" + start + ", @search='" + S.Sql.Encode(search) + "', @orderby=" + orderby + ", @dateStart=NULL, @dateEnd=NULL"));
            if(reader.Rows.Count > 0)
            {
                while (reader.Read())
                {
                    html += "<div class=\"row topic\">" +
                            "<div class=\"column title\">" + S.Sql.Decode(reader.Get("title")) + "</div>" +
                            "<div class=\"column btn\"><a href=\"/Dashboard/Topics/" + reader.GetInt("topicId") + "\" class=\"button green\">Read</a></div>" +
                            "</div>";
                }
            }
            else
            {
                html = "<div style=\"width:100%; padding:20px 0; text-align:center; font-size:24px;\">No Topics Available</div>";
            }

            response.inject = enumInjectTypes.replace;
            response.element = element;
            response.html = html;
            response.js = CompileJs();
            return response;
        }

        public Inject AddTopic(string element, int subjectId, string title, string description)
        {
            S.Sql.ExecuteNonQuery("EXEC AddTopic @subjectId=" + subjectId + ", @title='" + S.Sql.Encode(title) + "', @summary='" + S.Sql.Encode(description) + "'");
            return GetTopics(element, subjectId, 1, "", 1);
        }
        #endregion

        #region "Related Words"
        public List<Articles.AnalyzedWord> GetRelatedWordsForSubject(int subjectId, string search)
        {
            var articles = new Articles(S, S.Page.Url.paths);
            var scavange = new Scavenger(S, S.Page.Url.paths);
            var words = new List<Articles.AnalyzedWord>();
            var contents = scavange.GetContentFromWebSearch(search, 10, Scavenger.enumWebSearchEngines.wikipedia);
            foreach (var c in contents)
            {
                //analyze each piece of content
                var article = articles.Analyze(c.url, c.html);

                //combine words
                words = articles.CombineWordLists(words, article.words);
            }
            
            return words;
        }

        public Inject LoadRelatedWordsUI(int subjectId, int parentId, string search)
        {
            var response = new Inject();
            var words = GetRelatedWordsForSubject(subjectId, search);
            var serveArticles = new Articles(S, S.Page.Url.paths);
            var commonWords = serveArticles.GetCommonWords();
            var i = 0;
            var wordType = "";
            var article = new Articles.AnalyzedArticle();
            //only show important words & sort by importance
            words = words.Where(w => w.importance >= 7).OrderBy(w => 10000 - w.count).OrderBy(w => 100 - w.importance).ToList();
            foreach(var word in words)
            {
                if(word.importance >= 7)
                {
                    serveArticles.GetWordTypeClassNames(article, word, commonWords);
                    response.html += "<div class=\"word\"><div class=\"count\">" + word.count + "</div><div class=\"label\">" + word.word + "</div></div>\n";
                }
                i++;
            }
            response.element = "#subjects" + parentId + " .option-box";
            response.js = CompileJs();
            response.inject = enumInjectTypes.replace;
            return response;
        }
        #endregion
    }
}
