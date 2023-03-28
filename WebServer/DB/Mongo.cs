using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using WebServer.ChatGPT;

namespace WebServer.DB
{
    public class ChatSession
    {
        [BsonId]
        public ObjectId _id { get; set; }
        [BsonElement("title")]
        public string title { get; set; }
        [BsonElement("chats")]
        public string chats { get; set; }
    }
    [BsonIgnoreExtraElements]
    class Book
    {
        public string id;
        public string title;
        public string tag;
        public string info;
    }
    public class Mongo
    {

        private static MongoClient _client;
        private static IMongoDatabase _database;

        public static void EnsureMongoContent()
        {
            if (_client != null)
            {
                return;
            }
            string connStr = "mongodb://140.143.97.63:27017";
            _client = new MongoClient(connStr);
            _database = _client.GetDatabase("x-project");
        }
        // 增加数据并验证是否存在
        public static void Add<T>(T value)
        {
            EnsureMongoContent();
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.InsertOne(value);
            }

        }
        public static List<T> Where<T>(FilterDefinition<T> definition)
        {
            EnsureMongoContent();
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
                    return default;
                }
            }
        }

        public static void Update<T>(FilterDefinition<T> definition, UpdateDefinition<T> update)
        {
            EnsureMongoContent();
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


        public static void Delete<T>(FilterDefinition<T> definition)
        {
            EnsureMongoContent();
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                collection.DeleteOne(definition);
            }
        }
        public static long Count<T>()
        {
            EnsureMongoContent();
            lock (_client)
            {

                IMongoCollection<T> collection = _database.GetCollection<T>(typeof(T).Name);
                return collection.CountDocuments(FilterDefinition<T>.Empty);
            }
        }
    }
}
