using System.Collections.Specialized;
using ZyGame;

Server.Service.AddService<WebService>("/web");
Server.Service.AddService<FileService>("/file");
Server.Service.AddService<CmdService>("/cmd");
Server.Service.AddService<GatewayService>("/gate");
Server.Service.AddService<DBService>("/db");
Server.Service.AddService<ChatService>("/chat");
await Server.Service.Stratup(8080);
Console.ReadLine();
await Server.Service.ShutdownAll();