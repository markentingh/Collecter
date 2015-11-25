using System;

namespace Collector.SqlClasses
{
    public class User: SqlMethods
    {

        public User(Core CollectorCore) : base(CollectorCore) { }

        public SqlReader AuthenticateUser(string email, string salt)
        {
            SqlReader reader = new SqlReader();
            if (S.Sql.dataType == enumSqlDataTypes.SqlClient)
            {
                reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC AuthenticateUser @email='" + email + "', @salt='" + salt + "'"));
            }
            return reader;
        }
    }
}
