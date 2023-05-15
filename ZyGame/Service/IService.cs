namespace ZyGame.Service
{
    public interface IService
    {
    }

    public interface IInitService : IService
    {
        Task Startup();
        Task Shutdown();
    }

    public interface ILogicService : IService
    {
        Task<object> Recvie(string id, byte[] message);
    }

    public interface IHttpService : IService
    {
        Task<object> Post(byte[] bytes);
        Task<object> Get(QueryString collection);
    }
}
