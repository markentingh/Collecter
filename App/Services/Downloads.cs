using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collector.Services
{
    public class Downloads : Service
    {
        public Downloads(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        #region "Servers"
        public Inject AddServer(int type, string title, string settings)
        {
            var response = new Inject();
            var serverTitle = title;
            switch (type)
            {
                case 0: //local
                    //first, make sure the local server doesn't already exist
                    if((int)S.Sql.ExecuteScalar("EXEC DownloadServerExists @settings='" + settings + "'") == 0)
                    {
                        serverTitle = "Local Host";
                        S.Sql.ExecuteNonQuery("EXEC AddDownloadServer @type=" + type + ", @title='" + serverTitle + "', @settings='" + settings + "'");
                    }
                    else
                    {
                        S.Page.RegisterJS("err", "alert('You have already added your local server to the server list.');");
                    }
                    break;

                case 1: //web server

                    break;
            }

            response.inject = enumInjectTypes.replace;
            response.element = ".server-list .contents";
            response.html = LoadServersUI();
            response.js = CompileJs();
            return response;
        }

        public string LoadServersUI()
        {
            var htm = "";
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetDownloadServers"));
            if(reader.Rows.Count > 0)
            {
                var i = 0;
                while (reader.Read())
                {
                    htm += "<div class=\"row server\">" +

                        //check button
                        "<div class=\"btn\"><a href=\"javascript:\" onclick=\"S.downloader.buttons.serverSettings(" + i + ")\" class=\"button green\">Settings</a></div>" +

                        //title & url
                        "<div class=\"title\">" + reader.Get("title") + "</div>" +
                        "<div class=\"settings\">" + reader.Get("settings") + "</div>" + 
                        "</div>";
                    i++;
                }
            }

            return htm;
        }
        #endregion


    }
}
