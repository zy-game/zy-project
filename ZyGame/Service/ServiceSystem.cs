using System.Collections.Specialized;
using System.Text;
using System.Web;
using WebSocketSharp;
using WebSocketSharp.Server;
using ZyGame.Network;

namespace ZyGame.Service
{
    internal class ServiceSystem : Singleton<ServiceSystem>
    {
        private HttpServer sock;
        private Dictionary<string, Type> service_urls = new Dictionary<string, Type>();
        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        private Dictionary<string, WebSocketChannel> connectes = new Dictionary<string, WebSocketChannel>();

        public void AddService(Type type, string url)
        {
            service_urls.Add(url, type);
        }

        public async Task Startup(ushort port)
        {
            sock = new HttpServer(port);
            sock.OnGet += ResponseHttpRequest;
            sock.OnPost += ResponseHttpRequest;
            sock.OnOptions += ResponseHttpRequest;
            foreach (var item in service_urls)
            {
                string path = item.Key;
                sock.AddWebSocketService<WebSocketChannel>(item.Key, handle =>
                {
                    handle.path = path;
                    handle.OnClosed += OnDisconnected;
                    handle.OnOpened += OnConnected;
                    handle.OnMessaged += OnMessage;
                });
                IService service = Activator.CreateInstance(item.Value) as IService;
                services.Add(item.Value, service);
                if (service is IInitService init)
                {
                    await init.Startup();
                }
            }
            sock.Start();
            if (sock.IsListening)
            {
                Server.Console.WriteLine(string.Format("Listening on port {0}", sock.Port));
            }
        }
        private async void OnMessage(WebSocketChannel handle, MessageEventArgs args)
        {
            if (!service_urls.TryGetValue(handle.path, out Type type))
            {
                Server.Console.WriteLine("Not find the service:" + handle.path);
                return;
            }
            if (!services.TryGetValue(type, out IService service))
            {
                Server.Console.WriteLine("Not find the service:" + handle.path);
                return;
            }
            ILogicService logicService = service as ILogicService;
            if (logicService is null)
            {
                Server.Console.WriteLine("The Service Is NotImplemented ILogicService:" + handle.path);
                return;
            }
            string result = (await logicService.Recvie(handle.ID, args.RawData)).TryToJson();
            if (result is null || result.Length is 0)
            {
                return;
            }
            handle.Write(result);
        }

        private void OnConnected(WebSocketChannel handle)
        {
            if (connectes.ContainsKey(handle.ID))
            {
                return;
            }
            Server.Console.WriteLine("WebSocket Connected -> " + handle.ID);
            connectes.Add(handle.ID, handle);
        }

        private void OnDisconnected(WebSocketChannel handle, CloseEventArgs args)
        {
            if (!connectes.ContainsKey(handle.ID))
            {
                return;
            }
            Server.Console.WriteLine("WebSocket Disconnected -> " + handle.ID);
            connectes.Remove(handle.ID);
        }

        private async void ResponseHttpRequest(object sender, HttpRequestEventArgs e)
        {
            if (!service_urls.TryGetValue(e.Request.Url.LocalPath, out Type type) || !services.TryGetValue(type, out IService service))
            {
                e.Response.StatusCode = 404;
                e.Response.Close(UTF8Encoding.UTF8.GetBytes("Not Find The Service:" + e.Request.Url.LocalPath), true);
                return;
            }
            IHttpService httpService = service as IHttpService;
            string result = string.Empty;
            HttpMethod method = new HttpMethod(e.Request.HttpMethod);
            e.Response.StatusCode = 200;
            if (HttpMethod.Get == method)
            {
                using QueryString query = new QueryString();
                NameValueCollection collection = HttpUtility.ParseQueryString(e.Request.RawUrl);
                foreach (var item in collection.AllKeys)
                {
                    query.SetField(item, collection.Get(item));
                }
                result = (await httpService.Get(query)).TryToJson();
            }
            else if (HttpMethod.Post == method)
            {
                byte[] temp = e.Request.InputStream.ReadBytes((int)e.Request.ContentLength64);
                result = (await httpService.Post(temp)).TryToJson();
            }
            else if (HttpMethod.Options == method)
            {
                e.Response.Headers.Set("Access-Control-Allow-Origin", "*");
                e.Response.Headers.Set("Access-Control-Allow-Methods", "OPTIONS,POST,GET");
                e.Response.Headers.Set("Access-Control-Allow-Headers", "origin, x-requested-with,access-control-allow-methods,access-control-allow-origin,access-control-allow-headers,content-type");
                e.Response.Close();
                return;
            }
            if (result.IsNullOrEmpty())
            {
                e.Response.Close();
                return;
            }
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(result);
            e.Response.ContentLength64 = bytes.LongLength;
            e.Response.ContentType = "application/json";
            e.Response.Close(bytes, true);
        }

        public Task Shutdown(Type type)
        {
            IInitService service = GetService(type) as IInitService;
            if (service is null)
            {
                return Task.CompletedTask;
            }
            services.Remove(type);
            return service.Shutdown();
        }

        public Task ShutdownAll()
        {
            Task[] tasks = new Task[services.Count];
            int index = 0;
            foreach (var item in services.Keys)
            {
                tasks[index] = Shutdown(item);
            }
            return Task.WhenAll(tasks);
        }

        public T GetService<T>() where T : IService => (T)GetService(typeof(T));

        public IService GetService(Type type)
        {
            if (services.TryGetValue(type, out IService service))
            {
                return service;
            }
            return default;
        }

        public void Broadcast(byte[] message)
        {
            foreach (var item in service_urls.Keys)
            {
                sock.WebSocketServices[item].Sessions.Broadcast(message);
            }
        }

        public void Send(string id, byte[] message)
        {
            if (!connectes.TryGetValue(id, out WebSocketChannel channel))
            {
                Server.Console.WriteLine("not find the connected:" + id.ToString());
                return;
            }
            channel.Write(message);
        }

        public async Task<object> SendAsync(string id, byte[] message)
        {
            if (!connectes.TryGetValue(id, out WebSocketChannel channel))
            {
                Server.Console.WriteLine("not find the connected:" + id.ToString());
                return default;
            }
            return await channel.WaitingResponse(message);
        }
    }
}
