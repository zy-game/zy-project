using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerFramework.Actor
{
    class ActorRoot
    {
        HttpServer server;
        public ActorRoot(ActorNetType netType, string url, Type type)
        {
            server = new HttpServer(url, this);
        }
    }
    internal class ActorSystem : Singleton<ActorSystem>
    {
        private Dictionary<string, ActorRoot> roots = new Dictionary<string, ActorRoot>();

        public ActorSystem()
        {

        }



        public void Register<T>(ActorNetType netType, string url) where T : ActorChannel
        {
            if (!roots.TryGetValue(url, out ActorRoot root))
            {
                roots[url] = root = new ActorRoot(netType, url, typeof(T));
            }
        }

        public void SendSync(IMessage message)
        {

        }

        public Task<IMessage> SendAsync(IMessage message)
        {
            return default;
        }
    }

    public interface IMessage : IDisposable
    {

    }

    public interface IBaseActor : IDisposable
    {
        Task<IMessage> Recvie(IMessage message);
        void SendSync(IMessage message);
        Task<IMessage> SendAsync(IMessage message);
    }

    public enum ActorNetType : byte
    {
        TCP,
        UDP,
        HTTP,
    }

    public abstract class ActorChannel : IBaseActor
    {
        public abstract Task<IMessage> Recvie(IMessage message);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public void SendSync(IMessage message)
        {
            Server.Actor.SendSync(message);
        }

        public Task<IMessage> SendAsync(IMessage message)
        {
            return Server.Actor.SendAsync(message);
        }
    }

}
