using DotNetty.Transport.Channels;

namespace ZyGame.Actor
{
    public abstract class ActorBase : IDisposable
    {
        internal IChannelId id;
        public ActorRoot root { get; internal set; }


        public abstract Task<object> Recvie(object message);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public void SendSync(object message)
        {
            root.SendSync(id, message);
        }

        public Task<object> SendAsync(object message)
        {
            return root.SendAsync(id, message);
        }
    }

}
