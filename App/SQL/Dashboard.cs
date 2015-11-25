using System;

namespace Collector.SqlClasses
{
    public class Dashboard : SqlMethods
    {

        public Dashboard(Core CollectorCore) : base(CollectorCore) { }

        #region "Articles"

        public SqlReader GetArticles(int start = 1, int length = 20, int subject = 0)
        {
            SqlReader reader = new SqlReader();
            if (S.Sql.dataType == enumSqlDataTypes.SqlClient)
            {
                reader.ReadFromSqlClient(
                    S.Sql.ExecuteReader(
                    "EXEC GetArticles  @start=" + start + ", @length=" + length + ", @subject=" + subject
                    )
                );
            }
            return reader;
        }

        public SqlReader GetArticle(int articleId)
        {
            SqlReader reader = new SqlReader();
            if (S.Sql.dataType == enumSqlDataTypes.SqlClient)
            {
                reader.ReadFromSqlClient(
                    S.Sql.ExecuteReader(
                    "EXEC GetArticle @articleId=" + articleId.ToString()
                    )
                );
            }
            return reader;
        }

        public int AddArticle(string title, string summary, int subject = 0)
        {
            return (int)S.Sql.ExecuteScalar(
                "EXEC AddArticle @title='" + title + "', @subject=" + subject + ", @summary='" + summary + "'");
        }

        #endregion
    }
}
