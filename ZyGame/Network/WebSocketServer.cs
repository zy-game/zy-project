namespace ZyGame.Network
{
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using DotNetty.Codecs.Http;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Codecs.Http.WebSockets;
    using static DotNetty.Codecs.Http.HttpVersion;
    using static DotNetty.Codecs.Http.HttpResponseStatus;
    using DotNetty.Buffers;
    using System.Text;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DotNetty.Common.Utilities;
    public class WebSocketServer : IDisposable
    {
        private IChannel bootstrapChannel;
        private IEventLoopGroup bossGroup;
        private IEventLoopGroup workGroup;
        private ushort Port;
        private bool isOpenSSL;
        public Action OnOpened { get; }
        public Action OnClosed { get; }
        public Action OnPong { get; }
        public Action<byte[]> OnMessaged { get; }
        public WebSocketServer(ushort port, bool isOpenSSL)
        {
            this.Port = port;
            this.isOpenSSL = isOpenSSL;
        }

        public async Task Start()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            Server.Console.WriteLine($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            Server.Console.WriteLine($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            bossGroup = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();
            X509Certificate2 tlsCertificate = null;
            if (isOpenSSL)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(AppContext.BaseDirectory, "dotnetty.com.pfx"), "password");
            }
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
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
                        pipeline.AddLast(new WebHandler(this, isOpenSSL));
                    }));

                bootstrapChannel = await bootstrap.BindAsync(IPAddress.Loopback, Port);
                Server.Console.WriteLine("Open your web browser and navigate to " + $"{(isOpenSSL ? "https" : "http")}" + $"://127.0.0.1:{Port}/");
                Server.Console.WriteLine("Listening on " + $"{(isOpenSSL ? "wss" : "ws")}" + $"://127.0.0.1:{Port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await bootstrapChannel.CloseAsync();
                workGroup.ShutdownGracefullyAsync().Wait();
                bossGroup.ShutdownGracefullyAsync().Wait();
            }
        }

        public async void Dispose()
        {
            Server.Console.WriteLine(bootstrapChannel.LocalAddress.ToString() + " Closed");
            await bootstrapChannel.CloseAsync();
            await workGroup.ShutdownGracefullyAsync();
            await bossGroup.ShutdownGracefullyAsync();
        }
    }

    class WebHandler : SimpleChannelInboundHandler<object>
    {
        private Boolean IsSsl;
        private WebSocketServer server;
        WebSocketServerHandshaker handshaker;

        public WebHandler(WebSocketServer server, bool isSsl)
        {
            this.server = server;
            this.IsSsl = isSsl;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (ctx == null)
                return;
            if (msg is IFullHttpRequest request)
            {
                this.HandleHttpRequest(ctx, request);
            }
            else if (msg is WebSocketFrame frame)
            {
                this.HandleWebSocketFrame(ctx, frame);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // Handle a bad request.
            if (!req.Result.IsSuccess)
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, BadRequest));
                return;
            }

            // Allow only GET methods.
            if (!Equals(req.Method, HttpMethod.Get) && !Equals(req.Method, HttpMethod.Post) && !Equals(req.Method, HttpMethod.Options))
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, Forbidden));
                return;
            }
            Server.Console.WriteLine("HTTP METHOD:" + req.Method + ":" + req.Uri);
            // Handshake
            String url = GetWebSocketLocation(req);
            var wsFactory = new WebSocketServerHandshakerFactory(url, null, true, 5 * 1024 * 1024);
            this.handshaker = wsFactory.NewHandshaker(req);
            if (this.handshaker == null)
            {
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                this.handshaker.HandshakeAsync(ctx.Channel, req);
                var clientAddress = ctx.Channel.RemoteAddress.ToString();
                //todo new cnnected
            }
        }

        async void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            // Check for closing frame
            if (frame is CloseWebSocketFrame)
            {
                await this.handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                return;
            }

            if (frame is PingWebSocketFrame)
            {

                await ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                return;
            }

            if (frame is TextWebSocketFrame)
            {
                // Echo the frame
                int readableBytes = frame.Content.ReadableBytes;
                var msg = frame.Content.GetString(0, readableBytes, Encoding.UTF8);
                dynamic request = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(msg);
                // object response = await root.Recvie(ctx.Channel.Id, request);
                // if (response is null)
                // {
                //     return;
                // }
                frame = new TextWebSocketFrame();
                frame.Content.WriteBytes(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(default)));
                await ctx.WriteAsync(frame.Retain());
                return;
            }

            if (frame is BinaryWebSocketFrame)
            {
                // Echo the frame
                int readableBytes = frame.Content.ReadableBytes;
                var msg = frame.Content.GetString(0, readableBytes, Encoding.UTF8);
                dynamic request = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(msg);
                // object response = await root.Recvie(ctx.Channel.Id, request);
                // if (response is null)
                // {
                //     return;
                // }
                frame.Content.Clear();
                frame.Content.WriteBytes(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(default)));
                await ctx.WriteAsync(frame.Retain());
            }
        }

        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            if (res.Status.Code != 200)
            {
                IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
                res.Content.WriteBytes(buf);
                buf.Release();
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Origin"), "*");
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Methods"), "OPTIONS,POST,GET");
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Headers"), "origin, x-requested-with,access-control-allow-methods,access-control-allow-origin,access-control-allow-headers,content-type");
            // Send the response and close the connection if necessary.
            Task task = ctx.Channel.WriteAndFlushAsync(res);
            if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
            {
                task.ContinueWith((t, c) => ((IChannelHandlerContext)c).CloseAsync(),
                    ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine($"{nameof(WebHandler)} {0}", e);

            if (ctx != null)
            {
                ctx.CloseAsync();
            }
            //webSocketBroadCastEvent.Myevent -= Send;
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            //webSocketBroadCastEvent.Myevent -= Send;

            return base.CloseAsync(context);
        }
        string GetWebSocketLocation(IFullHttpRequest req)
        {
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            Debug.Assert(result, "Host header does not exist.");
            string location = value.ToString() + req.Uri;

            if (IsSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }
    }
}