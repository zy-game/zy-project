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
            string fileName = typeof(T).Name;
            return default;
        }
    }
}
