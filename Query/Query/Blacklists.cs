using System.Collections.Generic;

namespace Query
{
    public static class Blacklists
    {
        public static List<string> Domains()
        {
            return Sql.Populate<string>("Blacklist_Domains_GetList");
        }
    }
}
