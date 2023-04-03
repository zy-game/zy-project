using NLog;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using DotNetty.Common;
using System.Runtime.InteropServices;
using System.Runtime;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;

namespace ServerFramework.Actor
{
    class HttpServer
    {
        public string url;
        public ActorRoot root;
        public IEventLoopGroup bossGroup;
        public IEventLoopGroup workerGroup;
        public IChannel bootstrapChannel;
        public readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public HttpServer(string url, ActorRoot root)
        {
            ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
            this.url = url;
            this.root = root;
        }

        public async Task RunHttpServerAsync(int port)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap.Option(ChannelOption.SoBacklog, 8192);
                bootstrap.ChildHandler(new ActionChannelInitializer<DotNetty.Transport.Channels.IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new HttpServerCodec());
                    pipeline.AddLast(new HttpObjectAggregator(65536 * 5));
                    pipeline.AddLast(new HttpDecoder(this));
                }));

                bootstrapChannel = await bootstrap.BindAsync(port);
                Console.WriteLine("start http server success. listener port:[{}]", port);
            }
            catch (Exception e)
            {
                throw new Exception("start http server ERROR! \n" + e.ToString());
            }
        }

        public Task Start(int port)
        {
            return RunHttpServerAsync(port);
        }

        public async Task Stop()
        {
            await Task.WhenAll(
                bootstrapChannel.CloseAsync(),
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            LOGGER.Info("http server stoped");
        }
    }

}
