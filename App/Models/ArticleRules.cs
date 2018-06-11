namespace Collector.Models.Article
{
    public static class Rules
    {
        //rules array name prefix/suffix meanings:
        //ignore = used to ignore certain keywords
        //bad = used to score DOM elements negetively
        //good = used to score DOM elements positively

        //various separators used when splitting a string into an array
        public static string[] wordSeparators = new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", "'", ";", ":", "[", "]", "{", "}", "”", "“", "—", "_", "~", "…" };
        public static string[] sentenceSeparators = new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", ";", ":", "[", "]", "{", "}", "”", "“", "—", "_", "~", "…" };
        public static string[] separatorExceptions = new string[] { "'", "\"" };
        public static string[] scriptSeparators = new string[] { "{", "}", ";", "$", "=", "(", ")" };

        //used to identify words that may be a date
        public static string[] dateTriggers = new string[] {
            "published","written","posted",
            "january","february","march","april","may","june", "july","august",
            "september","october","november","december"
        };

        //used to identify various DOM elements that are used as titles
        public static string[] headerTags = new string[] { "h1", "h2", "h3", "h4", "h5", "h6" };

        //used to identify DOM elements that should be ignored in queries
        public static string[] ignoreTags = new string[] { "body" };

        //used to identify DOM elements that should be skipped when analyzing the document
        public static string[] skipTags = new string[] {
            "html", "head", "body", "meta", "title", "link", "script", "style"
        };

        //used to identify DOM elements that should not be included in the article
        public static string[] badTags = new string[]  {
            "head", "meta", "link", "applet", "area", "style",
            "audio", "canvas", "dialog", "small", "embed", "iframe",
            "input", "label", "nav", "object", "option", "s", "script", 
            "textarea", "video", "figure", "figcaption", "noscript"
        };

        //used to determine if a DOM element is used for advertisements or UI
        public static string[] badClasses = new string[] {
            "social", "advert", "menu", "keyword", "twitter", "replies", "reply",
            "nav", "search", "trending", "sidebar", "sidecontent", "discussion", "footer",
            "bread", "disqus", "callout", "toolbar", "masthead","addthis",
            "related", "-ad-", "ad-cont", "tags", "contacts", "popular", "promo",
            "banner", "subscri", "button", "reddit", "login", "signup",
            "signin", "recommend", "promot", "reading", "share", "sharing", "facebook",
            "poweredby", "powered-by", "invisible", "logo"
        };

        //used to find URLs that may be used for advertisements or UI buttons
        public static string[] badUrls = new string[] {
            "/ads/", "/ad/", "/click/", "/bait/", "refer", "javascript:"
        };
        
        //used to find vulgure language
        public static string[] badWords = new string[] {
            "shit", "crap", "asshole", "shitty", "bitch", "slut", "whore",
            "fuck", "fucking", "fucker", "fucked", "fuckers", "fucks"
        };

        //used to determine if a string should be flagged
        public static string[] badKeywords = new string[] {
            "disqus", "advertisement", "follow on twitter", "check out our", "announcement",
            "users", "sign up", "log in", "sign in", "reset pass", "subscribe", "learn more",
            "more stories", "click for", "update required", "update your browser", "supports html5",
            "support html5"
        };

        //used to determine if a small group of words ( < 5) is actually a menu item
        public static string[] badMenu = new string[] {
            "previous", "next", "post", "posts", "entry", "entries", "article", "articles",
            "more", "back", "go", "view", "about", "home", "blog", "rules", "resources", "skip",
            "create", "account", "sign in", "sign up", "log in", "login", "signup", "content",
            "jump", "contents", "comment", "comments", "prev"
        };

        //used to check DOM hierarchy tag names of potential menu items
        public static string[] menuTags = new string[]
        {
            "li", "a", "h1", "h2", "h3", "h4", "h5", "h6"
        };

        //used to determine if a sentence (or group of sentences) is part of a legal document
        public static string[] badLegal = new string[]
        {
            "must directly", "illegal", "distribute", "official", "promote", "piracy", "must include",
            "must be accompanied", "appropriate", "do not use", "promotion", "policy", "agreement",
            "provisions", "prohibited", "please report"
        };


        //used to find 2 or more words in a sentence that suggests the sentence is used after the end of an article
        public static string[] badTrailing = new string[] {
            "additional", "resources", "notes", "comment", "discuss", "post", "links",
            "share", "article"
        };

        public static string[] badRoles = new string[] { "application" };

        //used to check for malicious characters within a string
        public static string[] badChars = new string[] { "|", ":", "{", "}", "[", "]" };

        public static string[] domainSuffixes = new string[] { "com", "net", "org", "biz" };

        //used to remove common query key/value pairs from URLs
        public static string[] commonQueryKeys = new string[] { "ie", "utm_source", "utm_medium", "utm_campaign" };

        //used to verify if a string is in fact HTML
        public static string[] HtmlVerify = new string[] { "<div", "<html", "<a ", "<img ", "<p>" };
        
        //used to determine if a line break should be added after a block element
        public static string[] blockElements = new string[] {
            "address", "article", "aside", "blockquote", "dd", "div", "dl", "dt",
            "fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3",
            "h4", "h5", "h6", "header", "hr", "li", "main", "nav", "noscript", "ol",
            "output", "p", "pre", "section", "table", "tfoot", "ul" };
    }
}
