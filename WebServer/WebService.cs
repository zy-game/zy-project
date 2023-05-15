using ZyGame;
using ZyGame.Service;

class WebService : IHttpService, ILogicService
{
    class TestDb : DBEntity
    {
        public string name { get; set; }
    }
    private DBContext<TestDb> db_chat = new DBContext<TestDb>("");
    public Task<dynamic> Get(QueryString query)
    {
        return Task.FromResult<object>("Hello World");
    }

    public Task<object> Post(byte[] bytes)
    {
        return Task.FromResult<object>("Hello World");
    }

    public Task<object> Recvie(string id, byte[] message)
    {
        db_chat.Entities.Add(new TestDb() { name = id });
        db_chat.SaveChanges();
        return Task.FromResult<object>(new { msg = "Hello World => " + db_chat.Entities.Count() });
    }
}
