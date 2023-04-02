using MongoDB.Bson;

namespace WebServer.DB
{
    public class DBBook
    {
        public string id;
        public string title;
        public string tag;
        public string info;
    }

    public class DBCommand
    {
        public ObjectId id;
        public string title;
        public string text;
        public string time;
    }
}
