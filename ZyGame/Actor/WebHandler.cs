// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ZyGame.Actor
{

    using DotNetty.Codecs.Http;
    using DotNetty.Codecs.Http.WebSockets;
    using DotNetty.Transport.Channels;
    using static DotNetty.Codecs.Http.HttpVersion;
    using static DotNetty.Codecs.Http.HttpResponseStatus;
    using DotNetty.Buffers;
    using System.Text;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DotNetty.Common.Utilities;
    using System.Collections.Concurrent;


    public sealed class WebHandler : SimpleChannelInboundHandler<object>
    {
        private ActorRoot root;
        private static Boolean IsSsl;
        WebSocketServerHandshaker handshaker;
        public static void SetSsl(bool isSsl)
        {
            IsSsl = isSsl;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (ctx == null)
                return;
            if (msg is IFullHttpRequest request)
            {
                this.HandleHttpRequest(ctx, request);
            }
            else if (msg is WebSocketFrame frame)
            {
                this.HandleWebSocketFrame(ctx, frame);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // Handle a bad request.
            if (!req.Result.IsSuccess)
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, BadRequest));
                return;
            }

            // Allow only GET methods.
            if (!Equals(req.Method, HttpMethod.Get) && !Equals(req.Method, HttpMethod.Post) && !Equals(req.Method, HttpMethod.Options))
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, Forbidden));
                return;
            }
            Server.Console.WriteLine("HTTP METHOD:" + req.Method + ":" + req.Uri);
            // Handshake
            String url = GetWebSocketLocation(req);
            var wsFactory = new WebSocketServerHandshakerFactory(url, null, true, 5 * 1024 * 1024);
            this.handshaker = wsFactory.NewHandshaker(req);
            if (this.handshaker == null)
            {
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                this.handshaker.HandshakeAsync(ctx.Channel, req);
                var clientAddress = ctx.Channel.RemoteAddress.ToString();
                root = ActorSystem.instance.GetRoot(req.Uri);
                if (root is null)
                {
                    return;
                }
                root.AddChannel(ctx.Channel.Id, ctx.Channel);
            }
        }

        async void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            // Check for closing frame
            if (frame is CloseWebSocketFrame)
            {
                await this.handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                return;
            }

            if (frame is PingWebSocketFrame)
            {
                if (root is not null)
                {
                    root.Ping(ctx.Channel.Id);
                }
                return;
            }

            if (frame is TextWebSocketFrame)
            {
                // Echo the frame
                int readableBytes = frame.Content.ReadableBytes;
                var msg = frame.Content.GetString(0, readableBytes, Encoding.UTF8);
                dynamic request = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(msg);
                object response = await root.Recvie(ctx.Channel.Id, request);
                if (response is null)
                {
                    return;
                }
                frame = new TextWebSocketFrame();
                frame.Content.WriteBytes(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(response)));
                await ctx.WriteAsync(frame.Retain());
                return;
            }

            if (frame is BinaryWebSocketFrame)
            {
                // Echo the frame
                int readableBytes = frame.Content.ReadableBytes;
                var msg = frame.Content.GetString(0, readableBytes, Encoding.UTF8);
                dynamic request = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(msg);
                object response = await root.Recvie(ctx.Channel.Id, request);
                if (response is null)
                {
                    return;
                }
                frame.Content.Clear();
                frame.Content.WriteBytes(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(response)));
                await ctx.WriteAsync(frame.Retain());
            }
        }

        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            if (res.Status.Code != 200)
            {
                IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
                res.Content.WriteBytes(buf);
                buf.Release();
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Origin"), "*");
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Methods"), "OPTIONS,POST,GET");
            res.Headers.Add(AsciiString.Cached("Access-Control-Allow-Headers"), "origin, x-requested-with,access-control-allow-methods,access-control-allow-origin,access-control-allow-headers,content-type");
            // Send the response and close the connection if necessary.
            Task task = ctx.Channel.WriteAndFlushAsync(res);
            if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
            {
                task.ContinueWith((t, c) => ((IChannelHandlerContext)c).CloseAsync(),
                    ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine($"{nameof(WebHandler)} {0}", e);
            if (root is not null)
                root.RemoveChannel(ctx.Channel.Id);
            if (ctx != null)
                ctx.CloseAsync();

            //webSocketBroadCastEvent.Myevent -= Send;
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            //webSocketBroadCastEvent.Myevent -= Send;
            if (root is not null)
                root.RemoveChannel(context.Channel.Id);
            return base.CloseAsync(context);
        }
        static string GetWebSocketLocation(IFullHttpRequest req)
        {
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            Debug.Assert(result, "Host header does not exist.");
            string location = value.ToString() + req.Uri;

            if (IsSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }
    }
}


