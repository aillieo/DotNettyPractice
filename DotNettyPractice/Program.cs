using System;
using System.IO;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace DotNettyPractice
{
    public class Program
    {
        public static async Task RunServerAsync()
        {
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();
            
            try
            {
                var bootstrap = new ServerBootstrap()
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("frame-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("frame-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("handler", new SimpleChannelHandler());
                    }));

                IChannel boundChannel = await bootstrap.BindAsync(AppConfig.Port);

                Console.WriteLine($"Start listening : {boundChannel.LocalAddress}");
                Console.ReadLine();

                await boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public static void Main(string[] args)
        {
            RunServerAsync().Wait();
        }
    }
}