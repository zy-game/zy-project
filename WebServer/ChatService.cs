using ZyGame;
using ZyGame.Service;

class ChatService : IHttpService, IInitService
{
    class ChatRecord : DBEntity
    {

    }

    private DBContext<ChatRecord> db_chat;
    public Task<object> Get(QueryString collection)
    {

        throw new NotImplementedException();
    }

    public Task<object> Post(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public Task Startup()
    {
        db_chat = new DBContext<ChatRecord>("mongodb://140.143.97.63:27017");
        return Task.CompletedTask;
    }

    public Task Shutdown()
    {
        throw new NotImplementedException();
    }
}