using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyGame
{

    public static class Extension
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static T TryToObject<T>(this string value)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception e)
            {
                Server.Console.WriteError(e);
                return default;
            }
        }

        public static T TryToObject<T>(this byte[] value)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));
            }
            catch (Exception e)
            {
                Server.Console.WriteError(e);
                return default;
            }
        }

        public static string TryToJson(this object obj)
        {
            try
            {
                if (obj is null)
                {
                    return "{}";
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
            catch (Exception e)
            {
                Server.Console.WriteError(e);
                return obj.ToString();
            }
        }

        public static byte[] GetBuffer(this string value) => Encoding.UTF8.GetBytes(value);

        public static byte[] TryGetBuffer(this object obj)
        {
            return UTF8Encoding.UTF8.GetBytes(obj.TryToJson());
        }

        public static string GetString(this byte[] bytes) => UTF8Encoding.UTF8.GetString(bytes);

        public static T Getbject<T>(this byte[] bytes) => bytes.GetString().TryToObject<T>();
    }
}
