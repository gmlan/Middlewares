using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Com.Gmlan.Core.Extension
{
    public static class ObjectExtension
    {
        public static string FormatToString(this object obj, params string[] excludedProps)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            var infos = obj.GetType().GetProperties(flags);
            var sb = new StringBuilder();

            var typeName = obj.GetType().Name;
            sb.AppendLine(typeName);
            sb.AppendLine(string.Empty.PadRight(typeName.Length + 5, '='));

            foreach (var info in infos)
            {
                if (excludedProps.Length != 0 && excludedProps.ToList().Contains(info.Name))
                    continue;

                var value = info.GetValue(obj, null);
                sb.AppendFormat("{0}: {1}{2}", info.Name, value ?? "null", Environment.NewLine);
            }

            return sb.ToString();
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static object FromJson(this string str)
        {
            return JsonConvert.DeserializeObject(str);
        }

        public static T FromJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static object FromJson(this string str, Type type)
        {
            return JsonConvert.DeserializeObject(str, type);
        }

    }
}
