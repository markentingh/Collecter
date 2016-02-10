using System.Collections.Generic;
using System.Linq;

namespace Collector.Services
{
    public class Feeds : Service
    {

        public Feeds(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        public Inject AddFeed(string title, string url)
        {
            var inject = new Inject();

            //save new feed into database
            S.Sql.ExecuteNonQuery("EXEC AddFeed @title='" + S.Sql.Encode(title) + "', @url='" + S.Sql.Encode(url) + "'");
            S.Page.RegisterJS("addfeed", "alert('Feed added successfully');");

            //setup response
            inject.element = ".feeds .contents";
            inject.html = LoadFeedsUI();
            inject.js = CompileJs();
            return inject;
        }

        public string LoadFeedsUI()
        {
            var htm = "";
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetFeeds"));
            if(reader.Rows.Count > 0)
            {
                while (reader.Read())
                {
                    htm += "<div class=\"feed\">" +
                        "<div class=\"btn\"><a href=\"javascript:\" onclick=\"\" class=\"button green\">Check</a></div>" +
                        "<div class=\"title\">" + reader.Get("title") + "</div>" +
                        "<div class=\"url\">" + reader.Get("url") + "</div>" +
                        "</div>";
                }
            }

            return htm;
        }
    }
}
