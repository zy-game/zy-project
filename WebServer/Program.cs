using ZyGame;
using ZyGame.Actor;


Server.Actor.Register<Web>("/web");
Server.Actor.Register<File>("/file");
Server.Actor.Register<Cmd>("/cmd");
Server.Actor.Register<Gateway>("/gate");
Server.Actor.Startup("Web Server", 8080, false);
Server.Actor.BroadCast("/web", "222");
Console.ReadLine();
Server.Actor.Shutdown();
class Web : ActorBase
{
    public override async Task<object> Recvie(object message)
    {
        Server.Console.WriteLine(message);
        object next = await SendAsync(message);
        Server.Console.WriteLine(next);

        return message;
    }
}

class File : ActorBase
{
    public override Task<object> Recvie(object message)
    {
        return Task.FromResult(default(object));
    }
}

class Cmd : ActorBase
{
    public override Task<object> Recvie(object message)
    {
        return Task.FromResult(default(object));
    }
}

class Gateway : ActorBase
{
    public override Task<object> Recvie(object message)
    {
        return Task.FromResult(default(object));
    }
}