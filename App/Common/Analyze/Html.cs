using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.DOM;
using Collector.Models.Article;

namespace Collector.Common.Analyze
{
    public static class Html
    {
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

                for (var x = 1; x <= el.hierarchyIndexes.Length; x++)
                {
                    tabs += "    ";
                }
                htms.Append(tabs + htmelem + "\n");
            }
            return htms;
        }

        public static void GetContent(
            ref AnalyzedArticle article,
            ref List<AnalyzedTag> tagNames, 
            ref List<DomElement> textElements,
            ref List<DomElement> anchorElements,
            ref List<DomElement> headerElements,
            ref List<DomElement> imgElements,
            ref List<AnalyzedParentIndex> parentIndexes)
        {
            DomElement traverseElement;

            foreach (DomElement element in article.elements)
            {
                switch (element.tagName.ToLower())
                {
                    case "#text":
                        //sort text elements
                        textElements.Add(element);
                        traverseElement = element;
                        //add element's parent indexes to list
                        do
                        {
                            var parentIndex = parentIndexes.FindIndex(x => x.index == traverseElement.parent);
                            if (parentIndex >= 0)
                            {
                                //update existing parent index
                                var parent = parentIndexes[parentIndex];
                                parent.elements.Add(element.index);
                                parent.textLength += element.text.Length;
                                parentIndexes[parentIndex] = parent;
                            }
                            else
                            {
                                //create new parent index
                                var parent = new AnalyzedParentIndex();
                                parent.index = traverseElement.parent;
                                parent.elements = new List<int>();
                                parent.elements.Add(element.index);
                                parent.textLength = element.text.Length;
                                parentIndexes.Add(parent);
                            }
                            if (traverseElement.parent > -1)
                            {
                                //get next parent element
                                traverseElement = article.elements[traverseElement.parent];
                            }
                            else { break; }
                        } while (traverseElement.parent > -1);
                        break;
                    case "a":
                        //sort anchor links
                        anchorElements.Add(element);
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        headerElements.Add(element);
                        break;
                    case "img":
                        imgElements.Add(element);
                        break;
                    case "title":
                        if (article.title == "")
                        {
                            article.pageTitle = article.title = article.elements[element.index + 1].text.Trim();

                            //check for 404 error
                            if (article.title.IndexOf("404") >= 0)
                            {
                                return;
                            }
                        }
                        break;
                }

                //add new tag to list or incriment count of existing tag in list
                var tagIndex = tagNames.FindIndex(x => x.name == element.tagName);
                if (tagIndex >= 0)
                {
                    //incriment tag name count
                    var t = tagNames[tagIndex];
                    t.count += 1;
                    tagNames[tagIndex] = t;
                }
                else
                {
                    //add new tag to list
                    var t = new AnalyzedTag();
                    t.name = element.tagName;
                    t.count = 1;
                    tagNames.Add(t);
                }
            }
        }
    }
}
