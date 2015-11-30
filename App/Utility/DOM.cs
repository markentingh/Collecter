using System;
using System.Collections.Generic;
using Microsoft.AspNet.Diagnostics;

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
            public List<int> children;
            public int parent;
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
            var hierarchy = new List<string>();
            string strTag;
            int parentElement = -1;
            bool isClosingTag = false;
            bool isSelfClosing = false;
            bool isUnusedTag = false;

            //go through each tag in the array
            foreach (string tag in tags)
            {
                if(tag.Length <= 0) { continue; }
                var domTag = new DomElement();
                //domTag.children = new List<DomElement>();
                //domTag.style = new Dictionary<string, string>();
                isClosingTag = false;
                isSelfClosing = false;
                isUnusedTag = false;

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

                        if (strTag.Substring(strTag.Length - 2, 1) == "/")
                        {
                            isSelfClosing = true;
                        }

                        //check if tag is self-closing even if it
                        //doesn't include a forward-slash at the end
                        switch (domTag.tagName.ToLower())
                        {
                            case "br":
                            case "img":
                            case "input":
                            case "link":
                            case "meta":
                                isSelfClosing = true;
                                break;
                        }
                        if(domTag.tagName.Substring(0,1) == "!")
                        {
                            //comments & doctype self-closing tags
                            isSelfClosing = true;
                        }
                    }
                    else
                    {
                        if(strTag.Substring(0,1) == "/") {
                            //found closing tag
                            domTag.tagName = "#text";
                            isClosingTag = true;
                        }
                    }
                    //extract text after end of tag
                    domTag.text = tag.Substring(s1 + 1);

                    //check if domTag is unusable
                    if(domTag.text.Trim() == "" && isClosingTag == true)
                    {
                        isUnusedTag = true;
                    }
                }
                else
                {
                    //no end of tag, extract all text
                    domTag.tagName = "#text";
                    domTag.text = tag;
                    isSelfClosing = true;
                }
                if(domTag.tagName == null) { isUnusedTag = true; }

                

                if (isUnusedTag == false)
                {
                    //finally, add element to list
                    domTag.parent = parentElement;
                    if(parentElement > -1)
                    {
                        DomElement parent = Elements[parentElement];
                        if (parent.children == null)
                        {
                            parent.children = new List<int>();
                        }
                        parent.children.Add(Elements.Count);
                        Elements[parentElement] = parent;
                    }
                    //make current tag the parent
                    if (isSelfClosing == false && isClosingTag == false)
                    {
                        parentElement = Elements.Count;
                        hierarchy.Add(domTag.tagName);
                    }

                    //Console.WriteLine("Elements = " + Elements.Count + ", parent = " + domTag.parent + ", " + (domTag.tagName != null ? domTag.tagName : ""));
                    Console.WriteLine(">>> " + string.Join(" > ", hierarchy.ToArray()));

                    Elements.Add(domTag);  
                }

                if (isClosingTag == true)
                {
                    //go back one parent if this tag is a closing tag
                    if (parentElement >= 0)
                    {
                        //Console.WriteLine(parentElement + " is done, go back to " + Elements[parentElement].parent);
                        parentElement = Elements[parentElement].parent;
                        hierarchy.RemoveAt(hierarchy.Count - 1);
                    }
                }


                if (parentElement == -1)
                {

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
