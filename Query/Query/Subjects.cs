using System.Collections.Generic;
using System.Linq;

namespace Query
{
    public static class Subjects
    {
        public static int CreateSubject(int parentId, int grammartype, int score, string title, string breadcrumb)
        {
            return Sql.ExecuteScalar<int>("Subject_Create", new { parentId, grammartype, score, title, breadcrumb });
        }

        public static Models.Subject GetSubjectById(int subjectId)
        {
            return Sql.Populate<Models.Subject>("Subject_GetById", new { subjectId }).FirstOrDefault();
        }

        public static Models.Subject GetSubjectByTitle(string title, string breadcrumb)
        {
            return Sql.Populate<Models.Subject>("Subject_GetByTitle", new { title , breadcrumb }).FirstOrDefault();
        }

        public static void Move(int subjectId, int newParentId)
        {
            Sql.ExecuteNonQuery("Subject_Move", new { subjectId , newParentId });
        }

        public static List<Models.Subject> GetList(string subjectIds, int parentId = -1)
        {
            return Sql.Populate<Models.Subject>("Subjects_GetList", new { subjectIds , parentId });
        }
    }
}
