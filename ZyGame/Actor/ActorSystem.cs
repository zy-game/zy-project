using DotNetty.Codecs.Http;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ZyGame.Actor
{
    internal class ActorSystem : Singleton<ActorSystem>
    {
        private IChannel bootstrapChannel;
        private IEventLoopGroup bossGroup;
        private IEventLoopGroup workGroup;
        private Dictionary<string, ActorRoot> roots = new Dictionary<string, ActorRoot>();



        public void Register<T>(string url) where T : ActorBase
        {
            if (!roots.ContainsKey(url))
            {
                roots[url] = new ActorRoot(typeof(T));
            }
        }


        public async void Startup(string name, ushort Port, bool IsSsl)
        {
            Console.Title = name;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            Server.Console.WriteLine($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            Server.Console.WriteLine($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            bossGroup = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();
            X509Certificate2 tlsCertificate = null;
            if (IsSsl)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(AppContext.BaseDirectory, "dotnetty.com.pfx"), "password");
            }
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                WebHandler.SetSsl(IsSsl);
                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        pipeline.AddLast(new WebHandler());
                    }));

                bootstrapChannel = await bootstrap.BindAsync(IPAddress.Loopback, Port);
                Server.Console.WriteLine("Open your web browser and navigate to " + $"{(IsSsl ? "https" : "http")}" + $"://127.0.0.1:{Port}/");
                Server.Console.WriteLine("Listening on " + $"{(IsSsl ? "wss" : "ws")}" + $"://127.0.0.1:{Port}");

                foreach (var item in roots)
                {
                    Server.Console.WriteLine($"Opened Service: {item.Key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await bootstrapChannel.CloseAsync();
                workGroup.ShutdownGracefullyAsync().Wait();
                bossGroup.ShutdownGracefullyAsync().Wait();
            }
        }

        public ActorRoot GetRoot(string url)
        {
            if (roots.TryGetValue(url, out ActorRoot root))
            {
                return root;
            }
            return default;
        }

        public void BroadCast(string url, object Msg)
        {
            if (!roots.ContainsKey(url))
                return;
            roots[url].BroadCast(Msg);
        }



        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        public async void SendSync(string url, object message)
        {
            if (roots.TryGetValue(url, out ActorRoot root))
            {
                await root.Recvie(null, message);
                return;
            }
            //todo 调用远程服务
        }

        /// <summary>
        /// 发送消息并等待回执
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<object> SendAsync(string url, object message)
        {
            if (roots.TryGetValue(url, out ActorRoot root))
            {
                return await root.Recvie(null, message);
            }
            //todo 调用远程服务
            return default;
        }

        internal async void Shutdown()
        {
            foreach (var server in roots.Values)
            {
                server.Stop();
            }
            await bootstrapChannel.CloseAsync();
            await workGroup.ShutdownGracefullyAsync();
            await bossGroup.ShutdownGracefullyAsync();
            Server.Console.WriteLine("Server Shutdown");
        }
    }
}
