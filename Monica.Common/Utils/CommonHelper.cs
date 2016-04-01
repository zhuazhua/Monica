using System;
using Platinum.Common.Utils;

namespace Monica.Common.Utils
{
    public class CommonHelper
    {
        public static string FirstAlphabetToUpper(string targetString)
        {
            return targetString.Substring(0, 1).ToUpper() + targetString.Substring(1);
        }

        public static string FirstAlphabetToLower(string targetString)
        {
            return targetString.Substring(0, 1).ToLower() + targetString.Substring(1);
        }

        public static object Parse(Type type, string data)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, data);
            }
            if (type == typeof (int))
            {
                return int.Parse(data);
            }
            if (type == typeof (float))
            {
                return float.Parse(data);
            }
            if (type == typeof (double))
            {
                return double.Parse(data);
            }
            if (type == typeof (long))
            {
                return long.Parse(data);
            }
            if (type == typeof (DateTime))
            {
                return DateTimeHelper.Parse(data);
            }

            throw new Exception($"Unsupported type={type}");
        }
    }
}
