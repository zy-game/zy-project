namespace ZyGame
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
