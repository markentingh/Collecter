using System.Collections.Generic;

namespace Query.Blacklists
{
    public static class Domains
    {
        public static List<string> GetList()
        {
            return Sql.Populate<string>("Blacklist_Domains_GetList");
        }

        public static void Add(string domain)
        {
            Sql.ExecuteNonQuery("Blacklist_Domain_Add", new { domain });
        }
    }
}
