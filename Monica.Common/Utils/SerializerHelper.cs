using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Monica.Common.Pocos;

namespace Monica.Common.Utils
{
    public class SerializerHelper
    {
        public static MethodInfo DeserializeMethondInfo { get; } = typeof (SerializerHelper).GetMethod("Deserialize");

        private static readonly IFormatter Formatter = new BinaryFormatter();

        public static readonly JavaScriptSerializer JavaScriptSerializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

        public static DateTime FromUnixTime( long self)
        {
            var ret = new DateTime(1970, 1, 1);
            return ret.AddMilliseconds(self);
        }
        public static string XmlSerialize<T>(T t)
        {
            var stream = new MemoryStream();
            XmlSerialize(stream, t);
            using (var streamReader = new StreamReader(stream))
            {
               return streamReader.ReadToEnd();
            }
        }

        public static void XmlSerialize<T>(Stream stream, T t)
        {
            var xmlSerializer = new XmlSerializer(t.GetType());
            xmlSerializer.Serialize(stream, t);
        }

        public static void XmlSerialize<T>(XmlWriter writer, T t)
        {
            var xmlSerializer = new XmlSerializer(t.GetType());
            xmlSerializer.Serialize(writer, t);
        }


        public static void XmlSerialize<T>(StreamWriter stream, T t)
        {
            var xmlSerializer = new XmlSerializer(t.GetType());
            xmlSerializer.Serialize(stream, t);
        }

        public static T XmlDeserialize<T>(string data)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                return XmlDeSerialize<T>(stream);
            }
        }


        public static T XmlDeSerialize<T>(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var result = (T)xmlSerializer.Deserialize(stream);
            return result;
        }

        public static T XmlDeSerialize<T>(XmlReader reader)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var result = (T)xmlSerializer.Deserialize(reader);
            return result;
        }

        public static void Serialize(Stream stream, Object obj)
        {
            Formatter.Serialize(stream, obj);
        }

        public static object DeserizlizeByType(Type type, string data)
        {
            var deserializeMethod = DeserializeMethondInfo.MakeGenericMethod(type);
            var obj = deserializeMethod.Invoke(null, new object[] { data });
            return obj;
        }

        public static string Serialize(object obj)
        {
            return JavaScriptSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(String data)
        {
           
            return JavaScriptSerializer.Deserialize<T>(data);
        }

        public static T Clone<T>(T t)
        {
            return Deserialize<T>(Serialize(t));
        }

    }
}
