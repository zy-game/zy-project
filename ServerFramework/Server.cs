using MongoDB.Driver;
using ServerFramework.Actor;
using ServerFramework.Config;
using ServerFramework.DB;

namespace ServerFramework
{
    public static class Server
    {
        public sealed class DBService
        {
            public static T Insert<T>(T datable) => Mongo.instance.Add(datable);

            public static List<T> Where<T>(FilterDefinition<T> definition) => Mongo.instance.Where<T>(definition);

            public static void Update<T>(FilterDefinition<T> definition, UpdateDefinition<T> update) => Mongo.instance.Update(definition, update);

            public static void Delete<T>(FilterDefinition<T> definition) => Mongo.instance.Delete(definition);

            public static long Count<T>() => Mongo.instance.Count<T>();
        }

        public sealed class Config
        {
            public static T GetOrLoadConfig<T>() where T : class, new() => ConfigMgr.instance.GetOrLooad<T>();
        }

        public sealed class Actor
        {
            public static void RegisterActorChannel<T>(ActorNetType netType, string url) where T : ActorChannel => ActorSystem.instance.Register<T>(netType, url);

            public static void SendSync(IMessage message) => ActorSystem.instance.SendSync(message);

            public static Task<IMessage> SendAsync(IMessage message) => ActorSystem.instance.SendAsync(message);
        }
    }
}