using System.Collections.Generic;

namespace Collector.Models.Nodes
{
    //used for deserializing computed DOM object from WebBrowser console app
    public class Document
    {
        public string[] a;
        public Node dom;
    }

    public class Node
    {
        public string t = "";
        public int[] s = null;
        public Dictionary<int, string> a;
        public List<Node> c;
        public string v;
    }
}
