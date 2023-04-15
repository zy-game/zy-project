using MongoDB.Driver;
using ZyGame.Actor;
using ZyGame.Config;
using ZyGame.DB;

namespace ZyGame
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

        public sealed class Console
        {
            public static void WriteLine(object message) => InternalConsole.WriteLine(message);

            public static void WriteLineFormat(string format, params object[] message) => WriteLine(string.Format(format, message));

            public static void WriteWarning(object message) => InternalConsole.WriteWarning(message);

            public static void WriteWarningFormat(string format, params object[] message) => WriteWarning(string.Format(format, message));

            public static void WriteError(object message) => InternalConsole.WriteError(message);

            public static void WriteErrorFormat(string format, params object[] message) => WriteError(string.Format(format, message));
        }
        public sealed class Config
        {
            public static T GetOrLoadConfig<T>() where T : class, new() => ConfigMgr.instance.GetOrLooad<T>();
        }

        public sealed class Actor
        {
            public static void Startup(string name, ushort port, bool IsSsl) => ActorSystem.instance.Startup(name, port, IsSsl);

            public static void Shutdown() => ActorSystem.instance.Shutdown();

            public static void BroadCast(string url, object Msg) => ActorSystem.instance.BroadCast(url, Msg);

            public static void SendSync(string url, object message) => ActorSystem.instance.SendSync(url, message);

            public static Task<object> SendAsync(string url, object message) => ActorSystem.instance.SendAsync(url, message);

            public static void Register<T>(string url) where T : ActorBase => ActorSystem.instance.Register<T>(url);
        }
    }
}