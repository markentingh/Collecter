using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Net;
using Utility.Serialization;
using Utility.DOM;
using Collector.Models.Article;
using Collector.Models.Nodes;
using Utility.Strings;
using Utility;

namespace Collector.Common.Analyze
{
    public static class Html
    {
        #region "Step #1: Get HTML Document"
        public static AnalyzedArticle DeserializeArticle(string dom)
        {
            var article = new AnalyzedArticle();
            try
            {
                //deserialize object from string
                var node = (Document)Serializer.ReadObject(dom, typeof(Document));
                var html = new StringBuilder();
                var hierarchy = new List<int>();                
                var parser = new Parser("");

                //build DOM tree
                var elems = new List<DomElement>();
                var index = 0;
                Traverse(node.dom, ref index, elems, hierarchy, node.a, parser);
                parser.Elements = elems;
                article.elements = elems;
                article.rawHtml = FormatHtml(elems).ToString();
            }
            catch (Exception)
            {
                article.rawHtml = dom;
                throw new Exception();
            }
            

            return article;
        }

        private static DomElement Traverse(Node parent, ref int index, List<DomElement> elems, List<int> hierarchy, string[] attributes, Parser parser)
        {
            //create local copy of hierarchy
            var parentId = -1;
            int[] hier = new int[hierarchy.Count];
            if(hierarchy.Count > 0)
            {
                hierarchy.CopyTo(hier);
                parentId = hierarchy[hierarchy.Count() - 1];
            }
            
            //create DOM element
            var elem = new DomElement(parser);
            elem.index = index;
            elem.parent = parentId;
            elem.tagName = parent.t;
            elem.className = new List<string>();
            elem.hierarchyIndexes = hier.ToArray();
            elem.childIndexes = new List<int>();
            elem.style = new Dictionary<string, string>();
            elem.attribute = new Dictionary<string, string>();

            switch (elem.tagName)
            {
                case "#text": case "br": case "meta": case "link": case "hr": case "img":
                    elem.isSelfClosing = true;
                    break;
            }

            if(elem.tagName == "#text")
            {
                elem.text = parent.v;
            }

            //build style list for element
            if (parent.s != null)
            {
                switch (parent.s[0])
                {
                    case 0: elem.style.Add("display", "none"); break;
                    case 2: elem.style.Add("display", "inline"); break;
                    case 3: elem.style.Add("display", "inline-block"); break;
                }
                elem.style.Add("font-size", parent.s[1].ToString() + "px");
                if (parent.s[2] == 2)
                {
                    elem.style.Add("font-weight", "bold");
                }
                if (parent.s[3] == 1)
                {
                    elem.style.Add("font-style", "italic");
                }
            }

            //build attributes list
            if (parent.a != null)
            {
                foreach (var x in parent.a)
                {
                    if(attributes[x.Key] == "class")
                    {
                        elem.className = x.Value.Replace("  ", " ").Replace("  ", " ").Split(" ").ToList();
                    }
                    else
                    {
                        elem.attribute.Add(attributes[x.Key], x.Value);
                    }
                }
            }

            //append index to hierarchy list
            hier = hier.Append(index).ToArray();

            //add element to DOM elements list
            elems.Add(elem);

            if(parent.c != null)
            {
                //traverse all children
                foreach (var child in parent.c)
                {
                    index++;
                    var childElem = Traverse(child, ref index, elems, hier.ToList(), attributes, parser);
                    elem.childIndexes.Add(childElem.index);
                }
            }

            if(elem.isSelfClosing == false)
            {
                //add closing tag to DOM elements list
                index++;
                var closing = new DomElement(parser);
                closing.index = index;
                closing.tagName = "/" + parent.t;
                closing.hierarchyIndexes = hierarchy.ToArray();
                closing.childIndexes = new List<int>();
                closing.style = new Dictionary<string, string>();
                closing.attribute = new Dictionary<string, string>();
                closing.isClosing = true;
                elems.Add(closing);
            }
            return elem;
        }

        public static StringBuilder FormatHtml(List<DomElement> elements)
        {
            var htms = new StringBuilder();
            var htmelem = "";
            var tabs = "";
            foreach (var el in elements)
            {
                tabs = "";
                if (el.isClosing == true && el.isSelfClosing == false)
                {
                    //closing tag
                    htmelem = "<" + el.tagName + ">";
                }
                else
                {
                    if (el.tagName == "#text")
                    {
                        htmelem = el.text;
                    }
                    else
                    {
                        htmelem = "<" + el.tagName;
                        if(el.className.Count() > 0)
                        {
                            htmelem += " class=\"" + string.Join(" ", el.className) + "\"";
                        }
                        if (el.attribute != null)
                        {
                            if (el.attribute.Count > 0)
                            {
                                foreach (var attr in el.attribute)
                                {
                                    htmelem += " " + attr.Key + "=\"" + attr.Value + "\"";
                                }
                            }
                        }

                        if (el.isSelfClosing == true)
                        {
                            if (el.tagName == "!--")
                            {
                                htmelem += el.text + "-->";
                            }
                            else
                            {
                                htmelem += "/>";
                            }

                        }
                        else
                        {
                            htmelem += ">" + el.text;
                        }
                    }


                }

                for (var x = 0; x < el.hierarchyIndexes.Length - 1; x++)
                {
                    tabs += "    ";
                }
                htms.Append(tabs + htmelem + "\n");
            }
            return htms;
        }
        #endregion

        #region "Step #2: Get Article Info"
        public static void GetArticleInfoFromDOM(AnalyzedArticle article)
        {
            //get title
            article.title = "";
            var title = article.elements.Where(a => a.tagName == "title").FirstOrDefault();
            if(title != null)
            {
                var child = title.FirstChild;
                if(child != null)
                {
                    article.title = child.text;
                }
            }
            if(article.title == "")
            {
                //get title from header tags
                var headers = article.elements.Where(a =>
                {
                    switch (a.tagName)
                    {
                        case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                            return true;
                    }
                    return false;
                }).OrderBy(a => a.tagName).ToList();
                if(headers.Count > 0)
                {
                    for(var x = 0; x < headers.Count; x++)
                    {
                        var child = headers[x].FirstChild;
                        if(child != null)
                        {
                            if(child.text != "")
                            {
                                article.title = child.text;
                                break;
                            }
                        }
                    }
                }
            }

            //get page description
            var description = article.elements.Where(a => a.tagName == "meta" && a.attribute.ContainsKey("name") && a.attribute["name"].IndexOf("description") >= 0).FirstOrDefault();
            if(description != null)
            {
                article.summary = description.attribute["content"];
            }
        }
        #endregion

        #region "Step #3: Get Article Contents"
        public static int TraverseIndexes(AnalyzedArticle article, DomElement root, List<int> children)
        {
            var childNodes = root.Children();
            var maxDepth = 1;
            for (var x = 0; x < childNodes.Count; x++)
            {
                children.Add(childNodes[x].index);
                var depth = TraverseIndexes(article, childNodes[x], children);
                if(depth > maxDepth) { maxDepth = depth; }
            }
            return maxDepth;
        }

        public static void CheckTagForContamination(AnalyzedElement index, DomElement element, bool isChild, StringBuilder text)
        {
            if (element.tagName == "#text")
            {
                var txt = element.text.ToLower();
                text.Append(txt + " ");

                var words = SeparateWordsFromText(txt.ToLower());
                index.words += words.Length;

                if (words.Length == 1)
                {
                    index.badKeywords += Rules.badSingleWord.Where(a => txt.IndexOf(a) == 0).Count();
                }
                else if (words.Length < 5)
                {
                    var hierarchyTags = element.HierarchyTags();
                    if (hierarchyTags.Where(a => Rules.menuTags.Contains(a)).Count() > 0
                    )
                    {
                        index.badMenu += CountWordsInText(txt, Rules.badMenu);
                    }
                }
                if (words.Length <= 30)
                {
                    index.badKeywords += CountWordsInText(txt, Rules.badKeywords);
                    index.badKeywords += CountWordsInText(txt, Rules.badTrailing) > 2 ? 1 : 0;
                }

                //check for legal words
                index.badLegal = CountWordsInText(txt, Rules.badLegal);
            }
            else
            {
                if(isChild)
                {
                    //not a parent but instead a child element
                    index.tags++;

                    if (element.tagName == "a")
                    {
                        //check element for contamination from anchor links
                        if (element.attribute.ContainsKey("href"))
                        {
                            var href = element.attribute["href"];
                            index.badUrls += Rules.badUrls.Where(a => href.IndexOf(a) >= 0).Count() > 0 ? 1 : 0;
                        }
                    }else if(element.tagName == "img")
                    {
                        index.images++;
                    }
                }
                else
                {
                    if(element.tagName == "head")
                    {
                        index.badTags++;
                    }
                }

                //check for header
                if (element.tagName == "header" && element.Parent.tagName == "body")
                {
                    index.isBad = true;
                }

                //check element for contamination from class names
                if (element.className != null)
                {
                    var bad = Rules.badClasses.Where(badclass => !Rules.ignoreTags.Contains(element.tagName) && element.className.Where(classes => classes.IndexOf(badclass) == 0).Count() > 0).ToList();
                    index.badClasses += bad.Count();
                    for (var z = 0; z < bad.Count(); z++)
                    {
                        if (!index.badClassNames.Contains(bad[z]))
                        {
                            index.badClassNames.Add(bad[z]);
                        }
                    }
                    if(Rules.reallybadClasses.Where(badclass => element.className.Where(classes => classes.IndexOf(badclass) == 0).Count() > 0).Count() > 0)
                    {
                        index.isBad = true;
                    }
                }
            }
            //check for bad tags in hierarchy & children
            if (Rules.badTags.Contains(element.tagName))
            {
                index.badTags += 1;
                if (!index.badTagNames.Contains(element.tagName))
                {
                    index.badTagNames.Add(element.tagName);
                }
            }
        }

        public static void GetBestElementIndexes(AnalyzedArticle article, List<AnalyzedElement> bestIndexes, List<AnalyzedElement> badIndexes)
        {
            var allIndexes = new List<AnalyzedElement>();

            for (var x = 0; x < article.elements.Count; x++)
            {
                var parent = article.elements[x];

                if (Rules.skipTags.Contains(parent.tagName)) { continue; }

                var index = new AnalyzedElement()
                {
                    index = parent.index
                };
                var text = new StringBuilder();
                var hierarchy = new List<int>();

                //statistics
                var rootTextElements = 0;
                var textElements = 0;
                var aTags = 0;
                var pTags = 0;

                //check parent tag for contamination
                if (parent.hierarchyIndexes.Length < 2) { continue; }
                CheckTagForContamination(index, parent, true, text);
                if(index.badTags > 0 || index.badClasses > 0) {
                    badIndexes.Add(index);
                    continue;
                }

                foreach(var child in parent.Children())
                {
                    if(child.tagName == "#text")
                    {
                        rootTextElements++;
                    }
                }

                //check all child tags for contamination
                var maxDepth = TraverseIndexes(article, parent, hierarchy);

                //add parent hierarchy to list
                hierarchy = hierarchy.Concat(parent.hierarchyIndexes.ToList()).ToList();
                for (var y = 0; y < hierarchy.Count; y++)
                {
                    //check each element within the hierarchy
                    var el = article.elements[hierarchy[y]];
                    CheckTagForContamination(index, el, !parent.hierarchyIndexes.Contains(hierarchy[y]), text);

                    switch (el.tagName)
                    {
                        case "p": pTags++; break;
                        case "a": aTags++; break;
                        case "#text": textElements++; break;
                    }
                }

                //make sure menus are important enough for this element
                if(index.badMenu > 0)
                {
                    if(pTags > 1 || rootTextElements > 0) { index.badMenu = 0; }
                }
                if (index.badClasses + index.badTags +
                    index.badKeywords + index.badLegal + index.badMenu +
                    index.badUrls + index.words + index.images > 0)
                {
                    //add analyzed DOM element to list
                    allIndexes.Add(index);
                }

            } //check next element

            //filter best indexes that contain many legal words
            for (var x = 0; x < allIndexes.Count; x++)
            {
                var index = allIndexes[x];
                var children = new List<int>();
                TraverseIndexes(article, article.elements[index.index], children);
                var legalwords = allIndexes.Select(a => children.Contains(a.index) ? (a.badLegal > 1 ? a.badLegal : 0) : 0).Sum();

                if (legalwords > 5)
                {
                    //too many legal words
                    index.badLegal = legalwords;
                    index.isBad = true;
                    var remove = allIndexes.Where(a => children.Contains(a.index)).ToList();
                    for (var y =0; y < remove.Count; y++)
                    {
                        remove[y].isBad = true;
                    }
                }
            }

            //filter best indexes that contain menus
            for (var x = 0; x < allIndexes.Count; x++)
            {
                var index = allIndexes[x];
                var children = new List<int>();
                TraverseIndexes(article, article.elements[index.index], children);
                var indexes = allIndexes.Where(a => children.Contains(a.index));
                var menus = indexes.Select(a => a.badMenu).Sum();


                if (menus > 0)
                {
                    var others = 0;
                    for (var y = 0; y < children.Count; y++)
                    {
                        var el = article.elements[children[y]];
                        switch (el.tagName)
                        {
                            case "li":
                            case "a":
                            case "ul":
                            case "ol":
                            case "#text":
                                break;
                            default:
                                switch (article.elements[el.parent].tagName)
                                {
                                    case "li":
                                    case "a":
                                    case "ul":
                                    case "ol":
                                        break;
                                    default: others++; break;
                                }
                                break;
                        }
                    }

                    if (others / menus <= 1)
                    {
                        //found menus
                        index.isBad = true;
                        var remove = allIndexes.Where(a => children.Contains(a.index)).ToList();
                        for (var y = 0; y < remove.Count; y++)
                        {
                            remove[y].isBad = true;
                        }
                    }
                }
            }

            //filter best indexes that contain roles
            for (var x = 0; x < allIndexes.Count; x++)
            {
                var index = allIndexes[x];
                var elem = article.elements[index.index];
                if (elem.attribute.ContainsKey("role"))
                {
                    var role = elem.attribute["role"];
                    if (Rules.badRoles.Where(a => a == role).Count() > 0)
                    {
                        //make all child indexes contaminated
                        var children = new List<int>();
                        TraverseIndexes(article, elem, children);
                        var remove = allIndexes.Where(a => children.Contains(a.index)).ToList();
                        for (var y = 0; y < remove.Count; y++)
                        {
                            remove[y].isBad = true;
                        }
                    }
                }
            }

            //separate all indexes into best & bad indexes
            for (var x = 0; x < allIndexes.Count; x++)
            {
                var index = allIndexes[x];

                //check for bad elements
                if (index.badClasses > 0)
                {
                    var z = ((double)index.badClasses / (double)index.words);
                    if (z > 0.15)
                    {
                        index.badClassNames.Add(index.badClasses + " / " + index.words + " = " + z.ToString("#.###"));
                    }
                }
                if (index.isBad == true || (double)index.badClasses / (double)index.words > 0.15 || index.badTags > 0 || index.badUrls > 0 ||
                   index.badMenu > 0 || index.badKeywords > 0)
                {
                    //bad element
                    badIndexes.Add(index);
                }
                else
                {
                    //potentially good element
                    bestIndexes.Add(index);
                }
            }

            //clean memory of all indexes
            allIndexes = new List<AnalyzedElement>();

            //add up best indexes that contain other best indexes
            for (var x = 0; x < bestIndexes.Count; x++)
            {
                var index = bestIndexes[x];
                var children = new List<int>();
                TraverseIndexes(article, article.elements[index.index], children);
                index.bestIndexes = children.Where(a => bestIndexes.Where(b => b.index == a).Count() > 0).Count();
                index.badIndexes = children.Where(a => badIndexes.Where(b => b.index == a).Count() > 0).Count();
            }

            //sort best indexes by word count & by best index count within children
            bestIndexes = bestIndexes.OrderBy(a => (a.words * a.bestIndexes) * -1).ToList();

            //remove best indexes that are contaminated by bad text
            for (var x = 0; x < bestIndexes.Count; x++)
            {
                var index = bestIndexes[x];
                if(index.badIndexes > 0)
                {
                    if (index.badIndexes > index.bestIndexes)
                    {
                        bestIndexes.RemoveAt(x); x--;
                    }
                }
            }

            //remove best indexes that are contained within the top best indexes
            for (var x = 0; x < bestIndexes.Count; x++)
            {
                var index = bestIndexes[x];
                var children = new List<int>();
                TraverseIndexes(article, article.elements[index.index], children);

                var indexes = bestIndexes.FindAll(a => children.Contains(a.index));
                for (var y = 0; y < indexes.Count; y++)
                {
                    //if (y > x) { bestIndexes.Remove(indexes[y]); }
                }
            }

            bestIndexes = bestIndexes.OrderBy(a => a.index).ToList();
        }

        public static void GetArticleElements(AnalyzedArticle article, List<AnalyzedElement> bestIndexes, List<AnalyzedElement> badIndexes)
        {
            //build list of DOM elements that contains the article
            var bodyText = new List<int>();
            int parentId;
            var isFound = false;
            var isEnd = false;
            DomElement elem;
            var checkedIndexes = new List<int>();

            for (var x = bestIndexes.Count - 1; x >= 0; x--)
            {
                //all elements are a part of this parent element
                //get a list of text elements that are a part of the 
                //parent element
                parentId = bestIndexes[x].index;
                isFound = false;
                isEnd = false;
                for (var y = parentId + 1; y < article.elements.Count; y++)
                {
                    elem = article.elements[y];

                    if (elem.hierarchyIndexes.Contains(parentId))
                    {
                        //check if index was already handled
                        if (checkedIndexes.Contains(elem.index)) { continue; }
                        checkedIndexes.Add(elem.index);

                        //check for bad indexes
                        if (badIndexes.Where(a => a.index == elem.index || a.index == elem.parent).Count() > 0) { break; }


                        //determine which elements to include in the results
                        if (elem.tagName == "#text")
                        {
                            //add text
                            elem.text = WebUtility.HtmlDecode(elem.text);
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "br")
                        {
                            //add line break
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "img")
                        {
                            //add image
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "a")
                        {
                            //check for bad URLs in anchor link
                            if (elem.attribute.ContainsKey("href"))
                            {
                                if (Rules.badUrls.Where(a => elem.attribute["href"].IndexOf(a) >= 0).Count() > 0)
                                {
                                    badIndexes.Add(new AnalyzedElement()
                                    {
                                        index = elem.index,
                                        badUrls = 1
                                    });
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        //no longer part of parent id
                        isFound = true;
                        break;
                    }
                    if (isFound == true || isEnd == true) { break; }
                }
            }
            bodyText.Sort();
            article.body = bodyText;
        }

        public static string GetArticleText(AnalyzedArticle article)
        {
            //build article text
            DomElement domText;
            var text = new StringBuilder();
            for (var x = 0; x < article.body.Count; x++)
            {
                domText = article.elements[article.body[x]];
                if (domText.HierarchyTags().Where(h => Rules.headerTags.Contains(h)).Count() > 0)
                {
                    continue;
                }
                text.Append(domText.text.Trim() + " ");
            }
            return text.ToString();
        }

        public static List<string> GetSentences(string text)
        {
            var txt = "";
            var charA = "A".Asc();
            var charZ = "Z".Asc();
            var txt1 = "";
            var txt2 = "";
            char txt3;
            int sentenceStart = 0;
            bool foundSentence = false;
            var sentence = "";
            var sentences = new List<string>();
            var modtext = WebUtility.HtmlDecode(text.Replace("&nbsp;", ""))
                .Replace("... ", "{{p3}}").Replace(".. ", ". ").Replace(".. ", ". ").Replace("{{p3}}", "... ")
                .ReplaceAll("=+={1}=+=", Rules.sentenceSeparators);
            var texts = modtext.Replace("=+==+=", "=+=").Replace("=+==+=", "=+=")
                .Split(new string[] { "=+=" }, StringSplitOptions.RemoveEmptyEntries).Where(p => p.Trim() != "").ToArray();
            for (var x = 0; x < texts.Length; x++)
            {
                foundSentence = false;
                sentence = "";
                txt = texts[x].ToLower().Trim();
                if (txt == "") { continue; }

                //find end of sentence
                if (txt == ".")
                {
                    foundSentence = true;
                    //is period actually used for the end of a sentence 
                    //or is it used for an acronym or abbreviation instead?
                    if (texts.Length > x + 3)
                    {
                        //look ahead for a period
                        txt1 = texts[x + 1].Trim();
                        if (txt1 == "") { txt1 = " "; }
                        if (texts[x + 2].Trim() == ".")
                        {
                            if (txt1.IndexOf(" ") < 0)
                            {
                                //part of an abbreviation or acronym
                                foundSentence = false;
                            }
                            txt2 = texts[x + 3];
                            if (txt2.Length > 3)
                            {
                                //look ahead for website URLs
                                if (Rules.domainSuffixes.Contains(txt2.Substring(0, 3)) == true)
                                {
                                    txt3 = char.Parse(txt2.Substring(3, 1));
                                    if (txt3.CheckChar() == false)
                                    {
                                        //found a website URL, it is not an acronym like I had thought
                                        foundSentence = true;
                                    }
                                }
                            }

                        }
                        else if (Char.Parse(txt1.Substring(0, 1)).CheckChar(true, false, null, true) == true)
                        {
                            //look ahead for a capital letter
                        }
                        //text is a potential sentence part
                        if (x >= 2)
                        {
                            if (texts[x - 1].Trim() == ".")
                            {
                                foundSentence = false;
                            }
                        }
                        if (x < texts.Length - 2)
                        {
                            txt2 = texts[x + 1].Trim();
                            if (txt2.IndexOf("com") == 0)
                            {
                                if (1 == 1)
                                {

                                }
                            }
                            if (txt2.Length > 3)
                            {
                                //look for website URLs
                                if (Rules.domainSuffixes.Contains(txt2.Substring(0, 3)) == true)
                                {
                                    txt3 = Char.Parse(txt2.Substring(3, 1));
                                    if (txt3.CheckChar() == false)
                                    {
                                        //found a website URL
                                        foundSentence = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (txt == "!" || txt == "?" || txt == ":")
                {
                    foundSentence = true;
                }


                if (foundSentence == true)
                {
                    //look for quotes
                    if (texts.Length - 1 > x)
                    {
                        txt1 = texts[x + 1].Trim();
                        if (txt1 != "")
                        {
                            txt2 = txt1.Substring(0, 1);
                            if (txt2 == "\"" || txt2 == ",")
                            {
                                foundSentence = false;
                            }
                        }

                    }
                }


                if (foundSentence == true || x == texts.Length - 1)
                {
                    for (int z = sentenceStart; z <= x; z++)
                    {
                        sentence += texts[z];
                    }
                    //check sentence length
                    if (sentence.Length > 1)
                    {
                        txt = sentence.Trim();
                        if (txt.Length > 0)
                        {
                            txt1 = txt.Substring(0, 1);
                            txt2 = txt.Substring(txt.Length - 1, 1);
                            if (Rules.badChars.Contains(txt1) == true)
                            {
                                if (txt.Length > 1)
                                {
                                    txt = txt.Substring(1);
                                }
                                else
                                {
                                    txt = "";
                                }
                            }
                            if (Rules.badChars.Contains(txt2) == true)
                            {
                                if (txt2 != ":")
                                {
                                    txt = txt.Substring(0, txt.Length - 1);
                                }
                            }
                            if (txt != "")
                            {
                                sentences.Add(txt);
                                sentenceStart = x + 1;
                            }
                        }
                    }
                }
            }
            return sentences;
        }

        public static bool IsSentenceSeparator(string character, string[] exceptions = null)
        {
            if (exceptions != null)
            {
                if (exceptions.Contains(character) == true) { return false; }
            }
            return Rules.sentenceSeparators.Contains(character);
        }

        public static void GetImages(AnalyzedArticle article)
        {
            article.images = new List<AnalyzedImage>();
            for (var x = 0; x < article.body.Count; x++)
            {
                var elem = article.elements[article.body[x]];
                if (elem.tagName == "img" && elem.attribute.ContainsKey("src"))
                {
                    var img = new AnalyzedImage()
                    {
                        index = elem.index,
                        url = elem.attribute["src"]
                    };

                    img.filename = img.url.GetFilename();
                    img.extension = img.url.GetFileExtension().ToLower();

                    switch (img.extension)
                    {
                        case "jpg": case "jpeg": case "png": case "gif": case "tiff":
                        case "webp": case "bpg": case "flif": 
                            //only add supported image types
                            article.images.Add(img);
                            break;
                    }
                    
                }
            }
        }

        public static string[] SeparateWordsFromText(string text, string[] exceptions = null)
        {
            if (exceptions != null)
            {

                var ws = Rules.wordSeparators.Where(w => !exceptions.Contains(w)).ToArray();
                return text.ReplaceAll(" {1} ", ws).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
            }
            return text.ReplaceAll(" {1} ", Rules.wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
        }

        public static int CountWordsInText(string text, string[] words)
        {
            return words.Where(word => text.IndexOf(word) == 0 || (
                text.IndexOf(word) > 0 && 
                text.Substring(text.IndexOf(word) - 1, 1).ToCharArray()[0].CheckChar(true, true) == false
            )).Count();
        }

        #endregion

    }
}
