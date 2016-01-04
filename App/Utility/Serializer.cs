using System;
using System.IO;
using Newtonsoft.Json;

namespace Collector.Utility
{
    public class Serializer
    {
        private Core S;

        public Serializer(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public object ReadObject(string str, Type objType)
        {
            return JsonConvert.DeserializeObject(str, objType, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
        }

        public byte[] WriteObject(object obj, Formatting formatting = Formatting.Indented)
        {
            return S.Util.Str.GetBytes(JsonConvert.SerializeObject(obj, formatting, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        public string WriteObjectAsString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public void SaveToFile(object obj, string file)
        {
            File.WriteAllText(file, WriteObjectAsString(obj));
        }

        public object OpenFromFile(Type objType, string file)
        {
            return ReadObject(File.ReadAllText(file), objType);
        }
    }
}
