using System.Collections.Generic;
using System.Linq;

namespace Collector.Services.Dashboard
{
    public class Subjects : Service
    {

        public Subjects(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        #region "Subjects"
        public void AddSubject(string subjectList, int grammartype, string hierarchy, int score)
        {
            int parentId = 0;
            var subjects = subjectList.Replace(" , ", ",").Replace(", ", ",").Replace(" ,", ",").Split(',');
            var hier = new string[] { };
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
        #endregion

        #region "Subjects Results"
        public string GetSubjectsUI(int parentId)
        {
            SqlReader reader = GetSubjects(new string[] { "" }, parentId);
            if (reader.Rows.Count > 0)
            {
                var html = new List<string>();
                html.Add(
                    "<div class=\"accordion subjects\" id=\"subjects" + parentId + "\">\n" +
                        "<div class=\"title expanded\">Subjects</div>\n" +
                        "<div class=\"box expanded\">\n" +
                            "<div class=\"box-list\">"
                );

                while (reader.Read())
                {
                    html.Add(  "<div class=\"subject\" id=\"subject" + reader.GetInt("subjectId") + "\">\n" +
                                    "<a href=\"javascript:\" onclick=\"S.subjects.buttons.expandSubject('" + reader.GetInt("subjectId") + "', '" + parentId + "'); return false\">\n" +
                                    S.Util.Str.Capitalize(reader.Get("title")) + "</a><div class=\"sub\"></div>\n" +
                               "</div>\n");
                }

                html.Add(   "</div>" + 
                            "<div class=\"selection\">" + 
                                "<div class=\"label\"></div>" +
                                "<div class=\"option\"><a href=\"javascript:\" class=\"button\" onclick=\"\">Research</a></div>" +
                            "</div>\n" +
                        "</div>\n" +
                    "</div>\n");

                return string.Join("\n", html);
            }
            return "";
        }
        #endregion
    }
}
