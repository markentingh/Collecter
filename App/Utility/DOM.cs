using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collector.Utility.DOM
{
    public class Element
    {
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();
        public Dictionary<string, string> Style = new Dictionary<string, string>();
        public List<string> Classes = new List<string>();

        public string tagName = "";
        public string innerHTML = "";
        public bool ClosingTag = true;
        public string id = "";

        public Element(string tagname = "div", string tagId = "")
        {
            tagName = tagname;
            id = tagId;
        }

        public string Render()
        {
            string htm = "<" + tagName;

            //add id
            if(id != "") { htm += " id=\"" + id + "\""; }

            //add class names
            if(Classes.Count > 0)
            {
                htm += " class=\"";
                for (int x = 0; x < Classes.Count; x++)
                {
                   if(x > 0) { htm += " " + Classes[x]; }
                   else { htm += Classes[x]; } 
                }
                htm += "\"";
            }

            //add style
            if (Style.Count > 0)
            {
                htm += " style=\"";
                foreach(KeyValuePair<string, string> style in Style)
                {
                    htm += style.Key + ":" + style.Value + "; ";
                }
                htm += "\"";
            }

            //add attributes
            if (Attributes.Count > 0)
            {
                foreach (KeyValuePair<string, string> attr in Attributes)
                {
                    htm += " " + attr.Key + "=\"" + attr.Value + "\"";
                }
            }
            if(ClosingTag == false)
            {
                htm += "/>";
            }else
            {
                htm += ">" + innerHTML + "</" + tagName + ">";
            }
            return htm;
        }


    }

    public class Parser
    {
        public struct DomElement
        {
            public string tagName;
            public string text;
            public Dictionary<string, string> attribute;
            public Dictionary<string, string> style;
            public List<DomElement> children;
        }

        private Core S;
        public string rawHtml;
        public List<DomElement> Elements;

        public Parser(Core CollectorCore, string htm)
        {
            S = CollectorCore;
            rawHtml = htm;
            Elements = new List<DomElement>();
            Parse(htm);
        }

        public void Parse(string htm)
        {
            if(htm.Length <= 3) { return; }

            //first, find all opening tag characters
            //that aren't used for HTML elements
            int s1, s2, s3, s4, s5;
            string str1, str2, str3;
            string[] h = htm.Split('<');
            for(var x = 0; x < h.Length; x++)
            {
                if(h[x].Length > 0)
                {
                    str1 = h[x].Substring(0, 1);
                    if (!S.Util.Str.OnlyAlphabet(str1, new string[] { "/", "!" }))
                    {
                        h[x] = "{{op}}" + h[x];
                    }
                    else
                    {
                        h[x] = "<" + h[x];
                    }
                }
                
            }

            string curedHtm = string.Join("", h);

            //create array from HTML string using opening tag character
            string[] tags = curedHtm.Split('<');
            string strTag, strText;
            string[] arr;
            DomElement parentElement = new DomElement();
            bool addToParent = false;
            bool isClosingTag = false;
            bool unusedTag = false;
            parentElement.children = new List<DomElement>();

            //go through each tag in the array
            foreach (string tag in tags)
            {
                if(tag.Length <= 0) { continue; }
                var domTag = new DomElement();
                //domTag.children = new List<DomElement>();
                //domTag.style = new Dictionary<string, string>();
                isClosingTag = false;
                unusedTag = false;

                //find closing tag character
                s1 = tag.IndexOf(">");
                if (s1 >= 2)
                {
                    strTag = tag.Substring(0, s1);
                    //determine tag name, attributes, and style
                    s2 = strTag.IndexOf(" ");
                    if (s2 > 0)
                    {
                        //found tag name
                        domTag.tagName = strTag.Substring(0, s2);
                        //get attributes from tag
                        domTag.attribute = GetAttributes(strTag);
                    }
                    else
                    {
                        if(strTag.Substring(0,1) == "/") {
                            //found closing tag
                            isClosingTag = true;
                            domTag.tagName = "#text";
                        }
                    }
                    //extract text after end of tag
                    domTag.text = tag.Substring(s1 + 1);

                    //check if domTag is unusable
                    if(domTag.text.Trim() == "" && isClosingTag == true)
                    {
                        unusedTag = true;
                    }
                }
                else
                {
                    //no end of tag, extract all text
                    domTag.text = tag;
                }
                if(domTag.tagName == null) { unusedTag = true; }
                if(unusedTag == false)
                {
                    //finally, add element to list
                    if (addToParent == true)
                    {
                        if(parentElement.children == null)
                        {
                            parentElement.children = new List<DomElement>();
                        }
                        parentElement.children.Add(domTag);
                    }
                    else
                    {
                        Elements.Add(domTag);
                        parentElement = domTag;
                    }
                }

            }
        }

        public Dictionary<string, string> GetAttributes(string tag)
        {
            var attrs = new Dictionary<string, string>();
            int s1, s2, s3, s4, s5;
            string attrName, str2;
            string[] arr;
            s1 = tag.IndexOf(" ");
            if(s1 >= 1)
            {
                for (var x = s1; x < tag.Length; x++)
                {
                    //look for attribute name
                    s2 = tag.IndexOf("=", x);
                    s3 = tag.IndexOf(" ", x);
                    s4 = tag.IndexOf("\"", x);
                    s5 = tag.IndexOf("'", x);
                    if(s4 < 0) { s4 = tag.Length + 1; }
                    if (s5 < 0) { s5 = tag.Length + 2; }
                    if (s3 < s2 && s3 < s4 && s3 < s5)
                    {
                        //found a space first, then equal sign (=), then quotes
                        attrName = tag.Substring(s3 + 1, s2 - (s3 + 1));
                        //find type of quotes to use
                        if (s4 < s5)
                        {
                            //use double quotes
                            arr = tag.Substring(s4 + 1).Replace("\\\"","{{q}}").Split('"');
                            str2 = arr[0].Replace("{{q}}", "\\\"");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s4 + str2.Length + 1;
                        }else
                        {
                            //use single quotes
                            arr = tag.Substring(s5 + 1).Replace("\\'", "{{q}}").Split('\'');
                            str2 = arr[0].Replace("{{q}}", "\\'");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s5 + str2.Length + 1;
                        }
                    }
                }
            }
            return attrs;
        }
    }
}
