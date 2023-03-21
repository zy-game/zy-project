using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace WebServer
{

    public class Mongo
    {

        private static MongoClient _client;
        private static IMongoDatabase _database;

        public Mongo()
        {
            string connStr = "mongodb://140.143.97.63:27017";
            _client = new MongoClient(connStr);
            _database = _client.GetDatabase("x-project");
        }
        // 增加数据并验证是否存在
        public static void UpsertDocument<T>(FilterDefinition<T> filter, T document)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            if (collection.Find(filter).Any())
            {
                var update = Builders<T>.Update
                    .Set(d => d, document);

                collection.UpdateOne(filter, update);
            }
            else
            {
                collection.InsertOne(document);
            }
        }

        // 删除数据并验证是否存在
        public static void DeleteDocument<T>(FilterDefinition<T> filter)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            if (collection.Find(filter).Any())
            {
                collection.DeleteOne(filter);
            }
        }

        // 修改数据并验证是否存在
        public static void UpdateDocument<T>(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            if (collection.Find(filter).Any())
            {
                collection.UpdateOne(filter, update);
            }
        }

        // 查询数据
        public static T Find<T>(FilterDefinition<T> filter)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            return collection.Find(filter).ToList().First();
        }

        // 查询数据
        public static List<T> Where<T>(FilterDefinition<T> filter)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            return collection.Find(filter).ToList();
        }

        public static long Count<T>()
        {
            var collection = _database.GetCollection<T>(typeof(T).Name.ToLower());
            long count = collection.CountDocuments(Builders<T>.Filter.Empty);
            return count;
        }
    }
}
