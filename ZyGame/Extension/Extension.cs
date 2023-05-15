using MongoFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyGame
{
    public class DBEntity
    {
        public string Id { get; set; }
    }
    public class DBContext<T> : MongoDbContext where T : DBEntity
    {
        public DBContext(string url) : base(MongoDbConnection.FromConnectionString("mongodb://140.143.97.63:27017/MyDatabase")) { }
        public MongoDbSet<T> Entities { get; set; }

        protected override void OnConfigureMapping(MappingBuilder mappingBuilder)
        {
            //mappingBuilder.Entity<T>().HasProperty(m => m.Id, b => b.HasElementName("MappedName")).ToCollection("MyCustomEntities");
        }
    }
    public sealed class QueryString : IDisposable
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        public void Dispose()
        {
            values.Clear();
            GC.SuppressFinalize(this);
        }

        public T GetField<T>(string name)
        {
            if (!values.TryGetValue(name, out object value))
            {
                return default;
            }
            return (T)value;
        }

        public void SetField(string name, object value)
        {
            if (values.ContainsKey(name))
            {
                throw new Exception("the field is already exist");
            }
            values[name] = value;
        }
    }
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
                return obj.ToString();
            }
        }

        public static byte[] TryGetBuffer(this object obj)
        {
            return UTF8Encoding.UTF8.GetBytes(obj.TryToJson());
        }
    }
}
