namespace Collector.Models.Article
{
    public static class Rules
    {
        //-------------------------------------------------------------
        //rule name prefix/suffix meanings:
        //good = used to score DOM elements positively
        //bad = used to score DOM elements negetively
        //reallybad = used to score DOM elements extremely negetive
        //ignore = used to exclude certain keywords from scoring
        //-------------------------------------------------------------

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

        //used to identify header tags that may be part of the UI instead of the article
        public static string[] badHeaderWords = new string[] {
            "comments", "related articles", "related posts"
        };

        //used to identify DOM elements that should be skipped when analyzing the document
        public static string[] skipTags = new string[] {
            "html", "body", "meta", "title", "link", "script", "style"
        };

        //used to identify DOM elements that should not be included in the article
        public static string[] badTags = new string[]  {
            "head", "meta", "link", "applet", "area", "style",
            "audio", "canvas", "dialog", "small", "embed", "iframe",
            "input", "label", "nav", "object", "option", "s", "script", 
            "textarea", "video", "noscript", "footer"
        };

        //used to determine if a DOM element is used for advertisements or UI
        public static string[] badClasses = new string[] {
            "social", "advert", "menu", "keyword", "twitter", "replies", "reply",
            "search", "trending", "footer", "logo", "disqus", "popular", "contacts",
            "bread", "callout", "masthead", "addthis", "-ad-", "ad-cont", "tags",
            "banner", "subscri", "button", "reddit", "login", "signup", "promo",
            "signin", "recommend", "promot", "reading", "share", "sharing", "facebook",
            "poweredby", "powered-by", "invisible", "newsletter", "meta", "related",
            "nav", "navi", "menu", "sidebar", "toolbar", "sidecontent", "tab", 
            "embed", "hide", "hidden", "carousel", "overlay", "progress", "comment",
            "guestbook", "loading", "free-trial", "rating", "message"
        };

        //used to protect DOM elements that may be a part of the article
        public static string[] ignoreClasses = new string[]
        {
            "table"
        };

        //used to find URLs that may be used for advertisements or UI buttons
        public static string[] badUrls = new string[] {
            "/ads/", "/ad/", "/click/", "/bait/", "javascript:"
        };

        //used to find anchor links with flagged words they may be UI links
        public static string[] badLinkWords = new string[] {
            "more", "about", "back", "previous", "next", "link", "follow"
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
            "support html5", "support", "member", "this site", "exclusive", "podcast", "newsletter"
        };

        //used to determine if parent DOM element should be flagged for contamination
        public static string[] badKeywordsForParentElement = new string[]
        {
            "further discussion"
        };

        //used to determine if a single word should be flagged
        public static string[] badSingleWords = new string[] {
            "comments", "about", "articles", "members", "membership", "login", "log in", "signup", "sign up", 
            "signin", "sign in", "topics", "subscribe"
        };


        //used to determine if a small group of words ( < 5) is actually a menu item
        public static string[] badMenu = new string[] {
            "previous", "next", "post", "posts", "entry", "entries", "article", "articles",
            "more", "back", "view", "about", "home", "blog", "rules", "resources", "skip",
            "create", "account", "signin", "sign in", "login", "log in", "signup", "sign up", "content",
            "jump", "contents", "comment", "comments", "prev", "members", "articles", "membership",
            "topics", "subscribe"
        };

        //used to check DOM hierarchy tag names of potential menu items
        public static string[] menuTags = new string[]
        {
            "ul", "li"
        };

        //used to check DOM hierarchy tag names of potential text blocks
        public static string[] textTags = new string[]
        {
            "span", "p", "h1", "h2", "h3", "h4", "h5", "h6", "em"
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

        //check DOM element role attribute for bad role names
        public static string[] badRoles = new string[] { "application" };

        //used to check for malicious characters within a string
        public static string[] badChars = new string[] { "|", ":", "{", "}", "[", "]" };

        //domain suffixes
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
