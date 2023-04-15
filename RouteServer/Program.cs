// See https://aka.ms/new-console-template for more information
ZyGame.Server.Actor.Register<Route>("/route");

ZyGame.Server.Actor.Startup("Route Server", 8081, true);

class Route : ZyGame.Actor.ActorBase
{
    public override Task<object> Recvie(object message)
    {
        return default;
    }
}