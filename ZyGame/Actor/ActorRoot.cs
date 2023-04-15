using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using System.Collections.Concurrent;

namespace ZyGame.Actor
{
    public class ActorRoot
    {
        private Timer timer;
        private Type baseType;
        private ConcurrentQueue<object> MsgQueue;
        private ConcurrentQueue<ActorBase> actors = new ConcurrentQueue<ActorBase>();
        private Dictionary<IChannelId, IChannel> channels = new Dictionary<IChannelId, IChannel>();
        private ConcurrentDictionary<IChannelId, ConcurrentQueue<TaskCompletionSource<object>>> waittingList;

        public ActorRoot(Type type)
        {
            baseType = type;
            MsgQueue = new ConcurrentQueue<object>();
            waittingList = new ConcurrentDictionary<IChannelId, ConcurrentQueue<TaskCompletionSource<object>>>();
            timer = new Timer(Loop);
            timer.Change(5000, 1000);
        }

        private void Loop(object _)
        {
            if (MsgQueue.TryPeek(out object res))//Debug
            {
                string msg = Newtonsoft.Json.JsonConvert.SerializeObject(res);
                foreach (var item in channels.Values)
                {
                    Send(item, msg);
                }
            }
        }

        private async void Send(IChannel channel, string message)
        {
            WebSocketFrame frame = new TextWebSocketFrame(message);
            await channel.WriteAndFlushAsync(frame.Retain());
        }

        internal void AddChannel(IChannelId id, IChannel channel)
        {
            if (channels.ContainsKey(id))
            {
                Server.Console.WriteError("The Connected is Already Exists ：" + id);
                return;
            }
            channels.Add(id, channel);
            Server.Console.WriteLine("Connected：" + id);
        }

        internal void RemoveChannel(IChannelId id)
        {
            if (!channels.ContainsKey(id))
            {
                return;
            }
            channels.Remove(id);
            Server.Console.WriteLine("Disconnected：" + id);
        }

        internal void Ping(IChannelId id)
        {

        }

        public void BroadCast(object Msg)
        {
            MsgQueue.Enqueue(Msg);
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
        }

        internal async Task<object> Recvie(IChannelId id, object message)
        {
            if (waittingList.TryGetValue(id, out ConcurrentQueue<TaskCompletionSource<object>> queeus))
            {
                if (queeus.Count > 0)
                {
                    if (queeus.TryDequeue(out TaskCompletionSource<object> source))
                    {
                        source.SetResult(message);
                    }
                }
                return default;
            }
            if (!actors.TryDequeue(out ActorBase actor))
            {
                actor = (ActorBase)Activator.CreateInstance(baseType);
                actor.root = this;
            }
            actor.id = id;
            message = await actor.Recvie(message);
            actors.Enqueue(actor);
            return message;
        }

        public void SendSync(IChannelId id, object message)
        {
            if (!channels.TryGetValue(id, out IChannel channel))
            {
                return;
            }
            Send(channel, Newtonsoft.Json.JsonConvert.SerializeObject(message));
        }

        public Task<object> SendAsync(IChannelId id, object message)
        {
            if (!channels.TryGetValue(id, out IChannel channel))
            {
                return default;
            }
            TaskCompletionSource<object> waiting = new TaskCompletionSource<object>();
            if (!waittingList.TryGetValue(id, out ConcurrentQueue<TaskCompletionSource<object>> queues))
            {
                waittingList.TryAdd(id, queues = new ConcurrentQueue<TaskCompletionSource<object>>());
            }
            queues.Enqueue(waiting);
            Send(channel, Newtonsoft.Json.JsonConvert.SerializeObject(message));
            return waiting.Task;
        }
    }
}
