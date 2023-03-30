using MongoDB.Driver;
using WebServer.ChatGPT;

namespace WebServer.DB
{
    public class Mongo<T>
    {
        private static Lazy<Mongo<T>> _instance = new Lazy<Mongo<T>>(() => new Mongo<T>());
        public static Mongo<T> instance
        {
            get
            {
                return _instance.Value;
            }
        }
        private MongoClient _client;
        private IMongoDatabase _database;

        public Mongo()
        {
            _client = new MongoClient("mongodb://140.143.97.63:27017");
            _database = _client.GetDatabase("x-project");
        }
        // 增加数据并验证是否存在
        public void Add(T value)
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.InsertOne(value);
            }

        }
        public List<T> Where(FilterDefinition<T> definition)
        {
            lock (_client)
            {
                try
                {

                    IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                    var result = collection.Find(definition).ToList();
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new List<T>();
                }
            }
        }

        public void Update(FilterDefinition<T> definition, UpdateDefinition<T> update)
        {
            lock (_client)
            {
                try
                {

                    IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                    collection.UpdateOne(definition, update);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        public void Delete(FilterDefinition<T> definition)
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.DeleteOne(definition);
            }
        }
        public long Count()
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                return collection.CountDocuments(FilterDefinition<T>.Empty);
            }
        }
    }
}
