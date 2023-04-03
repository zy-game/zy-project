using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework.Config
{
    internal class ConfigMgr : Singleton<ConfigMgr>
    {
        public T GetOrLooad<T>() where T : class, new()
        {
            T result = default;
            string fileName = Environment.CurrentDirectory + "/" + typeof(T).Name + ".ini";
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(result = new T()));
                return result;
            }
            string fileData = File.ReadAllText(fileName);
            return result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileData);
        }
    }
}
