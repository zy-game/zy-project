namespace ZyGame
{
    public sealed class Queryable : IDisposable
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();
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
            return (T)Convert.ChangeType(value, typeof(T));
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
}
