namespace ZyGame.Network
{
    using System.Security.Cryptography.X509Certificates;
    using DotNetty.Codecs.Http;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Codecs.Http.WebSockets;
    using System.Text;
    using System;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;
    using System.Net.Security;
    using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;

    public class WebSocketClient : IDisposable
    {
        private IChannel bootstrapChannel;
        private IEventLoopGroup bossGroup;
        private bool isOpenSSL;
        private string address;

        public Action OnOpened { get; }
        public Action OnClosed { get; }
        public Action OnPong { get; }
        public Action<byte[]> OnMessaged { get; }
        public WebSocketClient(string address, bool isOpenSSL)
        {
            this.address = address;
            this.isOpenSSL = isOpenSSL;
        }
        public async void Dispose()
        {
            await bootstrapChannel.CloseAsync();
            await bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }

        public async Task Start()
        {
            Uri uri = new Uri(address);
            bossGroup = new MultithreadEventLoopGroup();
            X509Certificate2 cert = null;
            string targetHost = null;
            if (isOpenSSL)
            {
                cert = new X509Certificate2(Path.Combine(AppContext.BaseDirectory, "dotnetty.com.pfx"), "password");
                targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            }
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(bossGroup)
                    .Option(ChannelOption.TcpNodelay, true);
                bootstrap.Channel<TcpSocketChannel>();

                // Connect with V13 (RFC 6455 aka HyBi-17). You can change it to V08 or V00.
                // If you change it to V00, ping is not supported and remember to change
                // HttpResponseDecoder to WebSocketHttpResponseDecoder in the pipeline.
                var handler = new WebSocketClientHandler(this, WebSocketClientHandshakerFactory.NewHandshaker(uri, WebSocketVersion.V13, null, true, new DefaultHttpHeaders()));

                bootstrap.Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (cert != null)
                    {
                        pipeline.AddLast("tls", new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                    }

                    pipeline.AddLast(
                        new HttpClientCodec(),
                        new HttpObjectAggregator(8192),
                        WebSocketClientCompressionHandler.Instance,
                        handler);
                }));

                bootstrapChannel = await bootstrap.ConnectAsync(uri.Host, uri.Port);
                await handler.HandshakeCompletion;
                Console.WriteLine("WebSocket handshake completed.\n");
                OnOpened();
            }
            finally
            {
                Dispose();
            }
        }
    }
    class WebSocketClientHandler : SimpleChannelInboundHandler<object>
    {
        readonly WebSocketClient client;
        readonly WebSocketClientHandshaker handshaker;
        readonly DotNetty.Common.Concurrency.TaskCompletionSource completionSource;

        public WebSocketClientHandler(WebSocketClient client, WebSocketClientHandshaker handshaker)
        {
            this.client = client;
            this.handshaker = handshaker;
            this.completionSource = new DotNetty.Common.Concurrency.TaskCompletionSource();
        }

        public Task HandshakeCompletion => this.completionSource.Task;

        public override void ChannelActive(IChannelHandlerContext ctx) =>
            this.handshaker.HandshakeAsync(ctx.Channel).LinkOutcome(this.completionSource);

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine("WebSocket Client disconnected!");
            client.OnClosed();
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            IChannel ch = ctx.Channel;
            if (!this.handshaker.IsHandshakeComplete)
            {
                try
                {
                    this.handshaker.FinishHandshake(ch, (IFullHttpResponse)msg);
                    Console.WriteLine("WebSocket Client connected!");
                    this.completionSource.TryComplete();
                }
                catch (WebSocketHandshakeException e)
                {
                    Console.WriteLine("WebSocket Client failed to connect");
                    this.completionSource.TrySetException(e);
                    client.OnClosed();
                }
                return;
            }


            if (msg is IFullHttpResponse response)
            {
                throw new InvalidOperationException(
                    $"Unexpected FullHttpResponse (getStatus={response.Status}, content={response.Content.ToString(Encoding.UTF8)})");
            }

            if (msg is TextWebSocketFrame textFrame)
            {
                Console.WriteLine($"WebSocket Client received message: {textFrame.Text()}");
                client.OnMessaged(Encoding.UTF8.GetBytes(textFrame.Text()));
            }
            else if (msg is BinaryWebSocketFrame binary)
            {
                Console.WriteLine($"WebSocket Client received message: {Encoding.UTF8.GetString(binary.Content.Array)}");
                client.OnMessaged(binary.Content.Array);
            }
            else if (msg is PongWebSocketFrame)
            {
                Console.WriteLine("WebSocket Client received pong");
                client.OnPong();
            }
            else if (msg is CloseWebSocketFrame)
            {
                Console.WriteLine("WebSocket Client received closing");
                client.OnClosed();
                ch.CloseAsync();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            this.completionSource.TrySetException(exception);
            client.OnClosed();
            ctx.CloseAsync();
        }
    }
}