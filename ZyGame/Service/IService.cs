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

    public interface IwebSocketService : IService
    {
        Task<object> Recvie(string id, byte[] message);
    }

    public interface IHttpSocketService : IService
    {
        Task<object> Executedrequest(string path, byte[] bytes);
    }
}
