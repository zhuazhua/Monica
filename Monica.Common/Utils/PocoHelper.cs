using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Monica.Common.Utils
{
    public static class PocoHelper
    {
        public static void ParseDictionaryToProperties(object target, IReadOnlyDictionary<string, string> dictionary)
        {
            var properties = target.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (!dictionary.ContainsKey(prop.Name)) continue;

                var type = prop.PropertyType;

                if (type == new Double().GetType())
                {
                    // 处理百分数
                    var data = dictionary[prop.Name];
                    if (Regex.IsMatch(data, "%"))
                    {
                        data = data.Replace("%", "").Trim();
                        prop.SetValue(target, Convert.ToDouble(data)/100);
                        continue;
                    }
                    prop.SetValue(target, Convert.ToDouble(data));
                    continue;
                }
                if (type == new Int32().GetType())
                {
                    prop.SetValue(target, Convert.ToInt32(dictionary[prop.Name]));
                    continue;
                }
                if (type == new UInt32().GetType())
                {
                    prop.SetValue(target, Convert.ToUInt32(dictionary[prop.Name]));
                    continue;
                }
                if (type == new Int64().GetType())
                {
                    prop.SetValue(target, Convert.ToInt64(dictionary[prop.Name]));
                    continue;
                }
                if (type == new DateTime().GetType())
                {
                    try
                    {
                        prop.SetValue(target, Convert.ToDateTime(dictionary[prop.Name]));
                    }
                    catch (FormatException)
                    {
                        prop.SetValue(target, DateTime.ParseExact(dictionary[prop.Name], "yyyyMMdd",
                        CultureInfo.InvariantCulture));
                    }
                    continue;
                }
                if (type == new Boolean().GetType())
                {
                    prop.SetValue(target, Convert.ToBoolean(dictionary[prop.Name]));
                }
                else
                {
                    prop.SetValue(target, dictionary[prop.Name]);
                }
            }

        }


        /**/
        /// <summary>
        /// 把源对象里的各个Public Properties的内容直接赋值给目标对象（只是字段复制，两个对象的字段名和类型都必须一致）
        /// </summary>
        /// <param name="dest">目标对象</param>
        /// <param name="src">源对象</param>
        public static void CopyProperties(object dest, object src)
        {
            if (null == src) { throw new ArgumentNullException(nameof(src)); }
            if (null == dest) { throw new ArgumentNullException(nameof(dest)); }

            var srcProperties = src.GetType().GetProperties();
            var destProperties = dest.GetType().GetProperties();

            foreach (var destProperty in destProperties.Where(destProperty => destProperty.SetMethod != null && destProperty.GetMethod != null))
            {
                foreach (var srcProperty in srcProperties.Where(srcProperty => srcProperty.Name == destProperty.Name && srcProperty.PropertyType == destProperty.PropertyType))
                {
                    destProperty.SetValue(dest,srcProperty.GetValue(src));
                    break;
                }
            }
        }

        /**/
        /// <summary>
        /// 从一个对象里复制属性到另一个对象的同名属性
        /// </summary>
        /// <param name="dest">目标对象</param>
        /// <param name="src">源对象</param>
        /// <param name="fieldName">要复制的属性名字</param>
        public static void CopyProperty(ref object dest, object src, string fieldName)
        {
            if (null == src || null == dest || null == fieldName)
            {
                throw new ArgumentNullException("one argument is null!");
            }
            var srcType = src.GetType();
            var destType = dest.GetType();
            var srcInfo = srcType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var destInfo = destType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var val = srcInfo.GetValue(src);
            destInfo.SetValue(dest, val);

        }


        /**/
        /// <summary>
        /// 用于设置对象的属性值
        /// </summary>
        /// <param name="dest">目标对象</param>
        /// <param name="fieldName">属性名字</param>
        /// <param name="value">属性里要设置的值</param>
        public static void SetProperty(ref object dest, string fieldName, object value)
        {
            if (null == dest || null == fieldName || null == value)
            {
                throw new ArgumentNullException("one argument is null!");
            }
            var t = dest.GetType();
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            f.SetValue(dest, value);
        }


        /**/
        /// <summary>
        /// 反射打印出对象有的方法以及调用参数
        /// </summary>
        /// <param name="obj">传入的对象</param>
        public static void PrintMethod(ref object obj)
        {
            var t = obj.GetType();
            var mif = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (var i = 0; i < mif.Length; i++)
            {
                Trace.WriteLine("方法名字：  {0} ", mif[i].Name);
                Trace.WriteLine("方法来自：  {0}", mif[i].Module.Name);
                var p = mif[i].GetParameters();
                for (var j = 0; j < p.Length; j++)
                {
                    Trace.WriteLine("参数名:  " + p[j].Name);
                    Trace.WriteLine("参数类型:  " + p[j].ParameterType.ToString());
                }
                Trace.WriteLine("******************************************");
            }
        }
    }
}
