using Microsoft.AspNetCore.Authentication.Negotiate;
using ServerFramework;
using ServerFramework.Actor;
using WebServer.Web;

namespace WebServer
{
    class ChatGPTActor : ActorBase
    {
        public override Task<IMessage> Recvie(IMessage message)
        {
            return Task.FromResult(IMessage.Empty);
        }
    }

    class WebActor : ActorBase
    {
        public override Task<IMessage> Recvie(IMessage message)
        {
            return Task.FromResult(IMessage.Empty);
        }
    }

    class FileActor : ActorBase
    {
        public override Task<IMessage> Recvie(IMessage message)
        {
            return Task.FromResult(IMessage.Empty);
        }
    }

    class CommandActor : ActorBase
    {
        public override Task<IMessage> Recvie(IMessage message)
        {
            return Task.FromResult(IMessage.Empty);
        }
    }

    class GatewayActor : ActorBase
    {
        public override Task<IMessage> Recvie(IMessage message)
        {
            return Task.FromResult(IMessage.Empty);
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            Server.Actor.Register<ChatGPTActor>("/chat");
            Server.Actor.Register<WebActor>("/web");
            Server.Actor.Register<FileActor>("/file");
            Server.Actor.Register<CommandActor>("/cmd");
            Server.Actor.Register<GatewayActor>("/gate");
            Server.Actor.Startup("Web Server", 8080, string.Empty, true);
        }
    }
}
