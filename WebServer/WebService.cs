using ZyGame;
using ZyGame.Service;

class WebService : IHttpSocketService, IwebSocketService
{

    private DBContext<UserData> db_chat = new DBContext<UserData>("mongodb://140.143.97.63:27017/x_project");

    public Task<object> Recvie(string id, byte[] message)
    {
        db_chat.Entities.Add(new UserData() { name = id });
        db_chat.SaveChanges();
        return Task.FromResult<object>(new { msg = "Hello World => " + db_chat.Entities.Count() });
    }

    public Task<object> Executedrequest(string path, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    class UserData : DBEntity
    {
        public string name { get; set; }
    }
}
