using MongoDB.Driver;

namespace ZyGame.DB
{
    public class Mongo : Singleton<Mongo>
    {

        private MongoClient _client;
        private IMongoDatabase _database;

        public Mongo()
        {
            _client = new MongoClient("mongodb://140.143.97.63:27017");
            _database = _client.GetDatabase("x-project");
        }
        // 增加数据并验证是否存在
        public T Add<T>(T value)
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.InsertOne(value);
            }
            return value;
        }
        public List<T> Where<T>(FilterDefinition<T> definition)
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
                    Server.Console.WriteError(e);
                    return new List<T>();
                }
            }
        }

        public void Update<T>(FilterDefinition<T> definition, UpdateDefinition<T> update)
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
                    Server.Console.WriteError(e);
                }
            }
        }


        public void Delete<T>(FilterDefinition<T> definition)
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.DeleteOne(definition);
            }
        }
        public long Count<T>()
        {
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                return collection.CountDocuments(FilterDefinition<T>.Empty);
            }
        }
    }
}
