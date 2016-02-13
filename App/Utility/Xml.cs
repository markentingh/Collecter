using System;
using System.Xml;
using System.Text;
using System.IO;

namespace Collector.Utility
{
    public class Xml
    {

        public XmlDocument LoadXml(string xml)
        {
            // Encode the XML string in a UTF-8 byte array
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);

            // Put the byte array into a stream and rewind it to the beginning
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;

            // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(ms);
            }
            catch(Exception ex){}
            
            ms.Dispose();
            return xmlDoc;
        }

        public string GetAttribute(string name, XmlNode myNode)
        {
            if (!string.IsNullOrEmpty(myNode.Attributes[name].ToString())){
                return myNode.Attributes[name].Value;
            }
            return "";
        }

        public void SetAttribute(string name, string value, XmlNode myNode, XmlDocument myDoc)
        {
            if (myNode.Attributes.GetNamedItem(name) == null)
            {
                XmlAttribute newAttr = myDoc.CreateAttribute(name);
                newAttr.Value = value;
                myNode.Attributes.Append(newAttr);
            }
            else
            {
                myNode.Attributes[name].Value = value;
            }
        }

    }
}
