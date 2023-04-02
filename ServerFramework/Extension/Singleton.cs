using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework
{
    public class Singleton<T> where T : class, new()
    {
        private static Lazy<T> _instance = new Lazy<T>(() => new T());
        public static T instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
