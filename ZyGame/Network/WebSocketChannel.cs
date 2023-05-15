using WebSocketSharp;
using WebSocketSharp.Server;

namespace ZyGame.Network
{
    class WebSocketChannel : WebSocketBehavior
    {
        public string path;
        public event Action<WebSocketChannel> OnOpened;
        public event Action<WebSocketChannel, CloseEventArgs> OnClosed;
        public event Action<WebSocketChannel, MessageEventArgs> OnMessaged;

        private TaskCompletionSource<object> waiting;
        protected override void OnClose(CloseEventArgs e)
        {
            OnClosed(this, e);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Server.Console.WriteError(e.Message);
            CloseAsync();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsPing || "PING".Equals(e.Data))
            {
                Ping();
                return;
            }
            if (waiting is null)
            {
                OnMessaged(this, e);
                return;
            }
            waiting.SetResult(Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(e.Data));
        }

        protected override void OnOpen()
        {
            OnOpened(this);
        }

        public void Write(byte[] message)
        {
            Send(message);
        }

        public void Write(string message)
        {
            Send(message);
        }

        public Task WriteAsync(byte[] bytes)
        {
            TaskCompletionSource<bool> waiting = new TaskCompletionSource<bool>();
            SendAsync(bytes, state =>
            {
                waiting.SetResult(state);
            });
            return waiting.Task;
        }

        public Task WriteAsync(string message)
        {
            TaskCompletionSource<bool> waiting = new TaskCompletionSource<bool>();
            SendAsync(message, state =>
            {
                waiting.SetResult(state);
            });
            return waiting.Task;
        }

        public Task<object> WaitingResponse(byte[] bytes)
        {
            waiting = new TaskCompletionSource<object>();
            SendAsync(bytes, state =>
            {
                if (state is false)
                {
                    waiting.SetResult(default);
                    waiting = null;
                }
            });
            return waiting.Task;
        }
    }
}
