using NLog;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.Multipart;
using DotNetty.Transport.Channels;
using HttpMethod = DotNetty.Codecs.Http.HttpMethod;
using Newtonsoft.Json.Linq;

namespace ServerFramework.Actor
{
    class HttpDecoder : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private HttpServer server;
        public HttpDecoder(HttpServer server)
        {
            this.server = server;
        }
        protected override async void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {
            try
            {
                IFullHttpRequest request = msg;
                string uri = request.Uri;
                QueryStringDecoder queryStringDecoder = new QueryStringDecoder(uri);
                string path = queryStringDecoder.Path;
                if (!path.Equals(server.url))
                {
                    await ctx.CloseAsync();
                    return;
                }

                // chrome等浏览器会请求一次.ico
                if (uri.EndsWith(".ico"))
                {
                    await ctx.WriteAndFlushAsync(response(""));
                    return;
                }

                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                HttpMethod method = request.Method;

                QueryStringDecoder decoder = new QueryStringDecoder(request.Uri);
                foreach (var keyValuePair in decoder.Parameters)
                    paramMap.Add(keyValuePair.Key, keyValuePair.Value[0]);

                if (Equals(HttpMethod.Post, method))
                {
                    var headCType = request.Headers.Get(HttpHeaderNames.ContentType, null);
                    if (headCType == null)
                    {
                        await ctx.WriteAndFlushAsync(response(HttpHeaderNames.ContentType + " is null"));
                        await ctx.CloseAsync();
                        return;
                    }

                    string content_type = headCType.ToString().ToLower();
                    if (content_type != null && content_type.Equals("application/json"))
                    {
                        // json 格式
                        string str = request.Content.ToString(Encoding.UTF8);
                        var jsonNode = JObject.Parse(str);
                        if (jsonNode == null)
                            return;

                        var enumerator = jsonNode.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var keyValuePair = (KeyValuePair<string, JToken>)enumerator.Current;
                            if (paramMap.ContainsKey(keyValuePair.Key))
                            {
                                await ctx.WriteAndFlushAsync(response("参数重复了:" + keyValuePair.Key));
                                await ctx.CloseAsync();
                                return;
                            }
                            paramMap.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                        }
                    }
                    else
                    {
                        // key value 形式
                        HttpPostRequestDecoder decoder1 = new HttpPostRequestDecoder(request);
                        decoder1.Offer(request);
                        List<IInterfaceHttpData> parmList = decoder1.GetBodyHttpDatas();
                        foreach (var httpData in parmList)
                        {
                            if (httpData is IAttribute data)
                                paramMap.Add(data.Name, data.Value);
                        }

                        decoder1.Destroy();
                    }
                }

                string res = await handleHttpRequest(ctx.Channel.RemoteAddress.ToString(), uri, paramMap);
                await ctx.WriteAndFlushAsync(response(res));
            }
            catch (Exception e)
            {
                LOGGER.Error("http response error {} \n {}", e.Message, e.StackTrace);
                try
                {
                    await ctx.WriteAndFlushAsync(response(e.Message));
                    await ctx.CloseAsync();
                }
                catch (Exception)
                {
                    LOGGER.Error("HTTP连接关闭异常");
                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            ctx.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            LOGGER.Error("httpServerHandler Exception : {}, {}", cause.Message, cause);
            ctx.CloseAsync();
        }

        async Task<string> handleHttpRequest(string ip, string url, Dictionary<string, string> paramMap)
        {
            LOGGER.Info("收到来自[{}]的HTTP请求. 请求url:[{}]", ip, url);
            var str = new StringBuilder();
            str.Append("请求参数:");
            foreach (var parameter in paramMap)
            {
                if (parameter.Key.Equals(""))
                    continue;
                str.Append("'").Append(parameter.Key).Append("'='").Append(parameter.Value).Append("'  ");
            }
            LOGGER.Info(str.ToString());

            if (!paramMap.TryGetValue("cmd", out var cmd))
                return HttpResult.Undefine;

            try
            {
                var handler = HttpHandlerFactory.GetHandler(cmd, paramMap);
                if (handler == null)
                {
                    LOGGER.Warn($"http cmd handler 不存在：{cmd}");
                    return HttpResult.Undefine;
                }

                if (handler == null)
                {
                    LOGGER.Warn($"http cmd handler 为空：{cmd}");
                    return HttpResult.Undefine;
                }

                //验证
                var checkCode = handler.CheckSgin(paramMap);
                if (!string.IsNullOrEmpty(checkCode))
                    return checkCode;

                var ret = await Task.Run(() => { return handler.Action(ip, url, paramMap); });
                LOGGER.Warn("http result:" + ret);
                return ret;
            }
            catch (Exception e)
            {
                LOGGER.Error("执行http异常. {} {}", e.Message, e.StackTrace);
                return e.Message;
            }
        }

        /// <summary>
        /// 构造返回信息
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        DefaultFullHttpResponse response(string result)
        {
            var b = Encoding.UTF8.GetBytes(result);
            DefaultFullHttpResponse response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, Unpooled.WrappedBuffer(b));
            response.Headers.Add(HttpHeaderNames.ContentType, "text/html;charset=utf-8");
            response.Headers.Add(HttpHeaderNames.ContentLength, b.Length);
            return response;
        }
    }
    public class HttpResult
    {
        ///<summary>成功</summary>
        public const int Code_Success = 200;
        ///<summary>未定义的命令</summary>
        public const int Code_Undefine = 11;
        ///<summary>非法</summary>
        public const int Code_Illegal = 12;
        ///<summary>参数错误</summary>
        public const int Code_ParamErr = 13;
        ///<summary>验证失败</summary>
        public const int Code_CheckFailed = 14;
        ///<summary>操作失败</summary>
        public const int Code_ActionFailed = 15;

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public readonly static HttpResult Success = new HttpResult(Code_Success, "ok");
        public readonly static HttpResult Undefine = new HttpResult(Code_Undefine, "undefine command");
        public static HttpResult CreateOk(string retMsg = "")
        {
            return new HttpResult(Code_Success, retMsg);
        }

        public static HttpResult CreateErrorParam(string retMsg = "")
        {
            return new HttpResult(Code_ParamErr, retMsg);
        }

        public static HttpResult CreateActionFailed(string retMsg = "")
        {
            return new HttpResult(Code_ActionFailed, retMsg);
        }

        int code;
        string msg;
        readonly Dictionary<string, string> extraMap = new Dictionary<string, string>();
        public HttpResult(int retCode = 200, string retMsg = "ok")
        {
            code = retCode;
            msg = retMsg;
        }

        public string Get(string key)
        {
            if (extraMap.ContainsKey(key))
                return extraMap[key];
            return "";
        }

        /// <summary>
        /// key不能为code和msg
        /// </summary>
        public void Set(string key, string value)
        {
            if (key == "code" || key == "msg")
            {
                LOGGER.Error("HttpResult 额外信息key不能为:" + key);
                return;
            }
            extraMap[key] = value;
        }

        public override string ToString()
        {
            var json = new JObject();
            json["code"] = code;
            json["msg"] = msg;
            foreach (var kv in extraMap)
                json[kv.Key] = kv.Value;
            return json.ToString();
        }

        public static implicit operator string(HttpResult value)
        {
            return value.ToString();
        }
    }
}
