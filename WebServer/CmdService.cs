using ZyGame;
using ZyGame.Service;

class CmdService : IHttpSocketService
{
    public Task<object> Executedrequest(string path, byte[] bytes)
    {
        switch (path)
        {
            case CMDApiPath.HELP:
                return Task.FromResult<object>(CMDApiPath.GetCommandList());
        }
        return Task.FromResult<object>(new { msg = "Not find the command" });
    }

    class CMDApiPath
    {
        public const string HELP = "cmd/help";

        public static List<object> GetCommandList()
        {
            return new List<object>() { new { cmd = "help", text = "" } };
        }
    }
}
