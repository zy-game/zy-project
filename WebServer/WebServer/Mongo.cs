using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace WebServer
{

    public class Mongo
    {
        private MongoClient _client;
        private IMongoDatabase database;
        public Mongo()
        {
            string connStr = "mongodb://140.143.97.63:27017";
            _client = new MongoClient(connStr);
            database = _client.GetDatabase("x-project");
        }
        public void Add<T>(T value)
        {
            lock (_client)
            {
                IMongoCollection<T> collection = database.GetCollection<T>(typeof(T).Name);
                collection.InsertOne(value);
            }

        }
        public IEnumerable<T> Where<T>(Expression<Func<T, bool>> checker)
        {
            lock (_client)
            {
                try
                {
                    IMongoCollection<T> collection = database.GetCollection<T>(typeof(T).Name);
                    FilterDefinitionBuilder<T> builderFilter = Builders<T>.Filter;
                    FilterDefinition<T> filter = builderFilter.Where(checker);
                    var result = collection.Find(filter).ToEnumerable();
                    return result;
                }
                catch (Exception e)
                {
                    return default;
                }
            }
        }


        public void Delete<T>(Func<T, bool> checker)
        {
            lock (_client)
            {
                IMongoCollection<T> collection = database.GetCollection<T>(typeof(T).Name);
                collection.DeleteOne(temp => checker(temp));
            }
        }
        public long Count<T>()
        {
            lock (_client)
            {
                IMongoCollection<T> collection = database.GetCollection<T>(typeof(T).Name);
                return collection.CountDocuments(FilterDefinition<T>.Empty);
            }
        }


        private static Mongo mongo;
        public static Mongo Client
        {
            get
            {
                if (mongo == null)
                {
                    mongo = new Mongo();
                }
                return mongo;
            }
        }

        public static object Find(string id)
        {
            //4.获取数据集 collection；BsonDocument是在数据没有预先定义好的情况下使用的。
            IMongoCollection<BsonDocument> collection = Client.database.GetCollection<BsonDocument>("Player");

            //5.插入一条数据;
            var document = new BsonDocument { { "id", 2 }, { "name", "aa" } };
            collection.InsertOne(document);


            //6.查询数据1
            var res = collection.Find(new BsonDocument()).ToList(); //查询整个数据集
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }
            //6.查询数据2
            var res_a = collection.Find(new BsonDocument()).FirstOrDefault(); //查询当前数据集的第一条数据，没有则返回null
            Console.WriteLine(res_a);


            //6.升序降序查询3：
            var sort_ascending = Builders<BsonDocument>.Sort.Ascending("id"); //根据id升序
            var sort_descending = Builders<BsonDocument>.Sort.Descending("id"); //根据id降序
            var res_c = collection.Find(Builders<BsonDocument>.Filter.Lt("id", 10) & Builders<BsonDocument>.Filter.Gte("id", 2)).Limit(50).Sort(sort_ascending).ToCursor(); //查询id小于10，大于2的数据
            foreach (var item in res_c.ToEnumerable())
            {
                Console.WriteLine(item);
            }

            //7.更新数据，更新支持添加新的field, 如:
            collection.UpdateMany(Builders<BsonDocument>.Filter.Eq("id", 2), Builders<BsonDocument>.Update.Set("name", "hello"));   //将id字段为2的名字都改为“hello”;

            //8.删除
            collection.DeleteMany(Builders<BsonDocument>.Filter.Eq("id", 2));
            return "{}";
        }
    }
}
