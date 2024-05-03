namespace BackendServer;

using System.Net;
using System.Net.WebSockets;
public class Server
{
    private const string ServerAddressDefault = "127.0.0.1";
    private const int ServerPortDefault = 8080;
    private static readonly IMessageRouter _messageRouter = new MessageRouter();

    public static async Task Main(string[] args)
    {
        int serverPort = args.Length > 1 ? int.Parse(args[1]) : ServerPortDefault;

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{ServerAddressDefault}:{serverPort}/");
        httpListener.Start();

        Console.WriteLine($"Server listening on port {serverPort}...");

        var context = await httpListener.GetContextAsync();
        if (context.Request.IsWebSocketRequest)
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var webSocket = webSocketContext.WebSocket;

            RegisterHandlers();

            while (webSocket.State != WebSocketState.CloseReceived)
            {
                await HandleWebSocketRequest(webSocket);
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing normally", CancellationToken.None);
        }
        else
        {
            context.Response.StatusCode = 400;
            context.Response.Close();
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
