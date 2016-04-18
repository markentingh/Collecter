namespace Collector.Utility
{
    public class Util
    {
        private Core S;

        public Str Str;
        public Xml Xml;
        public Serializer Serializer;
        public Web Web;
        public Shell Shell;
        public Syndication RSS;

        public Util(Core CollectorCore)
        {
            S = CollectorCore;
            Str = new Str(S);
            Xml = new Xml();
            Serializer = new Serializer(S);
            Web = new Web(S);
            Shell = new Shell(S);
            RSS = new Syndication(S);
        }

        #region "Validation"

        public bool IsEmpty(object obj)
        {
            if(obj == null) { return true; }
            if (string.IsNullOrEmpty(obj.ToString())==true) { return true; }
            return false;
        }

        #endregion

        #region "Information"
        public string GetBrowserType()
        {
            string browser = S.Request.Headers["User-Agent"];
            browser = browser.ToLower();
            int major = 11;
            int minor = 0;
            if (browser.IndexOf("chrome") >= 0)
            {
                if (major > 10)
                {
                    return "chrome";
                }
                else
                {
                    return "legacy-chrome";
                }
            }
            else if (browser.IndexOf("firefox") >= 0)
            {
                if (major == 3 & minor >= 6)
                {
                    return "firefox";
                }
                else if (major > 3)
                {
                    return "firefox";
                }
                else
                {
                    return "legacy-firefox";
                }
            }
            else if (browser.IndexOf("safari") >= 0)
            {
                if (browser.IndexOf("iphone") >= 0)
                {
                    return "iphone";
                }
                else if (browser.IndexOf("ipad") >= 0)
                {
                    return "ipad";
                }
                else if (major <= 4)
                {
                    return "legacy-safari";
                }
                return "safari";
            }
            return "";
        }
        #endregion
        
    }
}
