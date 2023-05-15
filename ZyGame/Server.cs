using MongoDB.Driver;
using ZyGame.Config;
using ZyGame.Service;

namespace ZyGame
{
    public static class Server
    {
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

        public sealed class Service
        {
            public static Task Stratup(ushort port) => ServiceSystem.instance.Startup(port);
            public static void AddService<T>(string url) where T : IService => ServiceSystem.instance.AddService(typeof(T), url);

            public static void AddService(Type type, string url) => ServiceSystem.instance.AddService(type, url);

            public static Task Shutdown<T>() where T : IService => ServiceSystem.instance.Shutdown(typeof(T));

            public static Task Shutdown(Type type) => ServiceSystem.instance.Shutdown(type);

            public static Task ShutdownAll() => ServiceSystem.instance.ShutdownAll();

            public static T GetService<T>() where T : IService => ServiceSystem.instance.GetService<T>();

            public static IService GetService(Type type) => ServiceSystem.instance.GetService(type);

            public static void Broadcast(byte[] data) => ServiceSystem.instance.Broadcast(data);

            public static void Send(string id, byte[] message) => ServiceSystem.instance.Send(id, message);

            public static Task<object> SendAsync(string id, byte[] message) => ServiceSystem.instance.SendAsync(id, message);
        }
    }
}