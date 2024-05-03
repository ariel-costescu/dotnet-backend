namespace BackendServer;

using System;
using System.Net;
using System.Net.WebSockets;
using Serilog;

public class Server
{
    private const string ServerAddressDefault = "127.0.0.1";
    private const int ServerPortDefault = 8080;
    private static readonly IMessageRouter _messageRouter = new MessageRouter();

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        try
        {
            int serverPort = args.Length > 1 ? int.Parse(args[1]) : ServerPortDefault;

            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{ServerAddressDefault}:{serverPort}/");
            httpListener.Start();

            Log.Information($"Server listening on port {serverPort}...");

            RegisterHandlers();

            int maxConcurrentRequests = Environment.ProcessorCount;
            var incoming = new HashSet<Task>();
            for (int i = 0; i < maxConcurrentRequests; i++)
            {
                incoming.Add(httpListener.GetContextAsync());
            }

            CancellationTokenSource source = new();
            CancellationToken token = source.Token;

            while (!token.IsCancellationRequested)
            {
                Task t = await Task.WhenAny(incoming);
                incoming.Remove(t);

                if (t is Task<HttpListenerContext>)
                {
                    var context = (t as Task<HttpListenerContext>)?.Result;
                    incoming.Add(ProcessRequestAsync(context));
                    incoming.Add(httpListener.GetContextAsync());
                }
            }

            Log.Information("Server stopping...");
            httpListener.Stop();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static async Task ProcessRequestAsync(HttpListenerContext? context)
    {
        if (context != null)
        {
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                while (webSocket.State != WebSocketState.CloseReceived)
                {
                    await HandleWebSocketRequest(webSocket);
                }
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing normally", CancellationToken.None);
            }
            else
            {
                Log.Error("Didn't receive a WebSocket update request!");
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private static void RegisterHandlers()
    {
        _messageRouter.RegisterHandler("Login", LoginHandler.HandleLogin);
        _messageRouter.RegisterHandler("UpdateResources", UpdateResourcesHandler.HandleUpdateResources);
        _messageRouter.RegisterHandler("SendGift", SendGiftHandler.HandleSendGift);
    }

    private static async Task HandleWebSocketRequest(WebSocket webSocket)
    {
        var message = await MessageHelper.ReceiveMessage(webSocket);
        if (message != null)
        {
            await _messageRouter.RouteMessage(message, webSocket);
        }
    }


}
