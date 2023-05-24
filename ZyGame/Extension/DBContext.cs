using MongoFramework;

namespace ZyGame
{
    public class DBEntity
    {
        public string Id { get; set; }
    }
    public class DBContext<T> : MongoDbContext where T : DBEntity
    {
        public DBContext(string url) : base(MongoDbConnection.FromConnectionString(url)) { }
        public MongoDbSet<T> Entities { get; set; }

       
    }
}
