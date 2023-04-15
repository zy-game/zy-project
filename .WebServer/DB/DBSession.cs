using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebServer.DB
{
    public class DBSession
    {
        [BsonId]
        public ObjectId _id { get; set; }
        [BsonElement("title")]
        public string title { get; set; }
        [BsonElement("chats")]
        public string chats { get; set; }
    }
}
