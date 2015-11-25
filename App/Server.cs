using System;
using System.Collections.Generic;
using System.IO;

namespace Collector
{
    public class Server
    {
        ////////////////////////////////////////////////
        //Server     (for application-wide memory store)
        ////////////////////////////////////////////////

        public int requestCount = 0;
        public float requestTime = 0;
        public string sqlActive = "Azure";
        public string sqlConnection = "";
        public string saltPrivateKey = "?";
        private string _path = "";


        //Dictionary used for caching non-serialized objects, files from disk, or raw text
        //be careful not to leak memory into the cache while causing an implosion!
        public Dictionary<string, object> Cache = new Dictionary<string, object>();

        //Dictionary used for HTML scaffolding of various files on the server. 
        //Value for key/value pair is an array of HTML (scaffold["key"][x].htm), 
        //         separated by scaffold variable name (scaffold["key"][x].name),
        //         where data is injected in between each array item.
        public Dictionary<string, structScaffoldElement> Scaffold = new Dictionary<string, structScaffoldElement>();

        #region "System.UI.Web.Page.Server methods"
        public string path(string strPath = "")
        {
            if(_path == "") { _path = Path.GetFullPath("config.json").Replace("config.json", ""); }
            return _path + strPath.Replace("/", "\\");
        }

        public string MapPath(string strPath = "") { return path(strPath); }

        public string UrlDecode(string strPath)
        {
            return Uri.UnescapeDataString(strPath.Replace("+", " "));
        }

        public string UrlEncode(string strPath)
        {
            return Uri.EscapeDataString(strPath.Replace(" ","+"));
        }
        
        #endregion

        public string OpenFile(string file)
        {
            if(Cache.ContainsKey(file) == true)
            {
                return (string)Cache[file];
            }
            else
            {
                string data = File.ReadAllText(MapPath(file));
                Cache.Add(file, data);
                return data;
            }
        }

        public void SaveFile(string file, string data, bool saveToDisk = true)
        {
            if(Cache.ContainsKey(file) == true)
            {
                Cache[file] = data;
            }
            else
            {
                Cache[file] = data;
            }
            if(saveToDisk == true)
            {
                File.WriteAllText(MapPath(file), data);
            }
        }
    }


}
