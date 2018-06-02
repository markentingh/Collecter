using System;
using System.Collections.Generic;
using Utility.DOM;

namespace Collector.Models.Article
{
    public class AnalyzedArticle
    {
        public int id;
        public int feedId;
        public string rawHtml;
        public string url;
        public string domain;
        public string pageTitle;
        public string title;
        public string summary;
        public int relevance;
        public int importance;
        public int totalWords;
        public int totalSentences;
        public int totalParagraphs;
        public int totalImportantWords;
        public int totalBugsOpen;
        public int totalBugsResolved;
        public int yearStart;
        public int yearEnd;
        public List<int> years;
        public bool fiction;
        public List<DomElement> elements;
        public AnalyzedTags tags;
        public List<AnalyzedTag> tagNames;
        public List<AnalyzedParentIndex> parentIndexes;
        public List<AnalyzedWord> words;
        public List<AnalyzedPhrase> phrases;
        public List<ArticleSubject> subjects;
        public List<int> body;
        public List<DomElement> bodyElements;
        public List<string> sentences;
        public AnalyzedAuthor author;
        public DateTime publishDate;
        public List<AnalyzedPerson> people;
    }

    public struct AnalyzedTags
    {
        public List<AnalyzedText> text;
        public List<int> anchorLinks;
        public List<int> headers;
    }

    public struct AnalyzedTag
    {
        public string name;
        public int count;
        public int[] index;
    }

    public struct AnalyzedText
    {
        public int index;
        public List<AnalyzedWordInText> words;
        public TextType type;
        public List<PossibleTextType> possibleTypes;
    }

    public struct AnalyzedWord
    {
        public int id;
        public string word;
        public int count;
        public int importance;
        public bool suspicious;
        public WordType type;
        public WordCategory category;
        public bool apostrophe;
    }

    public struct AnalyzedPhrase
    {
        public int id;
        public string phrase;
        public int[] words;
        public int count;
    }

    public struct AnalyzedWordInText
    {
        public string word;
        public AnalyzedWord[] relations;
        public int index;
    }

    public struct AnalyzedImage
    {
        public string url;
        public int relavance;
    }

    public struct AnalyzedAuthor
    {
        public string name;
        public AuthorSex sex;
    }

    public struct AnalyzedFile
    {
        public string filename;
        public string fileType;
    }

    public struct AnalyzedParentIndex
    {
        public List<int> elements;
        public int index;
        public int textLength;
    }

    public struct AnalyzedElementCount
    {
        public int index;
        public int count;
    }

    public struct PossibleTextType
    {
        public TextType type;
        public int count;
    }

    public struct ArticleSubject
    {
        public int id;
        public int parentId;
        public string title;
        public WordType type;
        public string[] breadcrumb;
        public int[] hierarchy;
        public List<int> parentIndexes;
        public int count;
        public int score;
    }

    public struct AnalyzedPerson
    {
        public string fullName;
        public string firstName;
        public string middleName;
        public string lastName;
        public string surName;
        public int[] references; //word indexes within article words (he, she, his, hers, him, her, he'd, she'd, he's, she's, etc...)
    }

    public struct ArticleHtmlList
    {
        public string html;
        public List<string> list;
        public int id;
    }

    public enum TextType
    {
        mainArticle = 0,
        authorName = 1,
        publishDate = 2,
        comment = 3,
        advertisement = 4,
        linkTitle = 5,
        menuTitle = 6,
        header = 7,
        copyright = 8,
        script = 9,
        useless = 10,
        style = 11,
        anchorLink = 12,
        menuItem = 13
    }

    public enum WordType
    {
        none = 0,
        verb = 1,
        adverb = 2,
        noun = 3,
        pronoun = 4,
        adjective = 5,
        article = 6,
        preposition = 7,
        conjunction = 8,
        interjection = 9,
        punctuation = 10
    }

    public enum WordCategory
    {
        person = 0,
        place = 1,
        thing = 2,
        year = 3

    }

    public enum AuthorSex
    {
        female = 0,
        male = 1
    }

    public static class Rules
    {
        public static string[] wordSeparators = new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", "'", ";", ":", "[", "]", "{", "}", "”", "“", "—", "_", "~", "…" };
        public static string[] sentenceSeparators = new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", ";", ":", "[", "]", "{", "}", "”", "“", "—", "_", "~", "…" };

        public static string[] scriptSeparators = new string[] { "{", "}", ";", "$", "=", "(", ")" };

        public static string[] dateTriggers = new string[] {
            "published","written","posted",
            "january","february","march","april","may","june", "july","august","september","october","november","december" };

        public static string[] nonSentenceTags = new string[] { "h1", "h2", "h3", "h4", "h5", "h6", "title" };

        public static string[] badTags = new string[]  {
            "applet", "area", "audio", "canvas", "dialog", "small",
            "embed", "footer", "iframe", "input", "label", "nav",
            "object", "option", "s", "script", "style", "textarea", "video" };

        public static string[] badArticleTags = new string[]  {
            "applet", "area", "audio", "canvas", "dialog", "small", "nav",
            "embed", "footer", "iframe", "input", "label", "nav", "header", "head",
            "object", "option", "s", "script", "style", "title", "textarea",
            "video", "form", "figure", "figcaption","noscript" };

        public static string[] badAttributes = new string[] { "id" };

        public static string[] badClasses = new string[] {
            "head", "social", "side", "advert", "menu", "comment", "tag", "keyword",
            "nav", "logo", "list", "link", "search", "form", "topic", "feature",
            "filter", "categor", "bread", "credit", "foot", "disqus", "callout",
            "graphic", "image", "photo", "addthis", "tool", "separat",
            "related", "ad-", "item", "return", "mobile", "home", "about", "hidden",
            "semantic"};

        public static string[] badPhotoCredits = new string[] { "photo", "courtesy", "by", "copyright" };

        public static string[] badMenu = new string[] { "previous", "next", "post", "posts", "entry", "entries", "article", "articles", "more", "back", "go", "view", "about", "home", "blog" };

        public static string[] badTrailing = new string[] { "additional", "resources", "notes", "comment" };

        public static string[] badChars = new string[] { "|", ":", "{", "}", "[", "]" };

        public static string[] domainSuffixes = new string[] { "com", "net", "org", "biz" };

        public static string[] commonQueryKeys = new string[] { "ie", "utm_source", "utm_medium", "utm_campaign" };

        public static string[] HtmlVerify = new string[] { "<div", "<html", "<a ", "<img ", "<p>" };

        public static string[] suspiciousWords = new string[] { "copyright", "posts", "entry", "entries", "article", "articles", "home", "blog", "stories", "menu", "comments", "navigate", "trademark" };
    }
    

}
