using System.Collections.Generic;

namespace Collector.Query
{
    public class Subjects : global::Query.QuerySql
    {
        public int CreateSubject(int parentId, int grammartype, int score, string title, string breadcrumb)
        {
            return Sql.ExecuteScalar<int>(
                "Subject_Create",
                new Dictionary<string, object>()
                {
                    {"parentId", parentId },
                    {"grammartype", grammartype },
                    {"score", score },
                    {"title", title },
                    {"breadcrumb", breadcrumb }
                }
            );
        }

        public Models.Subject GetSubjectById(int subjectId)
        {
            var list = Sql.Populate<Models.Subject>(
                "Subject_GetById",
                new Dictionary<string, object>()
                {
                    {"subjectId", subjectId }
                }
            );
            if(list.Count > 0) { return list[0]; }
            return null;
        }

        public Models.Subject GetSubjectByTitle(string title, string breadcrumb)
        {
            var list = Sql.Populate<Models.Subject>(
                "Subject_GetByTitle",
                new Dictionary<string, object>()
                {
                    {"title", title },
                    {"breadcrumb", breadcrumb }
                }
            );
            if (list.Count > 0) { return list[0]; }
            return null;
        }

        public void Move(int subjectId, int newParentId)
        {
            Sql.ExecuteNonQuery("Subject_Move",
                new Dictionary<string, object>()
                {
                    {"subjectId", subjectId },
                    {"newParent", newParentId }
                }
            );
        }

        public List<Models.Subject> GetList(string subjectIds, int parentId = -1)
        {
            return Sql.Populate<Models.Subject>(
                "Subjects_GetList",
                new Dictionary<string, object>()
                {
                    {"subjectIds", subjectIds },
                    {"parentId", parentId }
                }
            );
        }
    }
}
