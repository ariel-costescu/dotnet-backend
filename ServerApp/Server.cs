using System.Net;
using System.Net.WebSockets;
using System.Text;

public class Server
{
    private const string ServerAddressDefault = "127.0.0.1";
    private const int ServerPortDefault = 8080;
    private const string SubProtocol = "websocket";
    private const int ReceiveBufferSize = 1024;
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

    private static async Task HandleWebSocketRequest(WebSocket webSocket)
    {
        var message = await ReceiveMessage(webSocket);
        if (message != null)
        {
            await _messageRouter.RouteMessage(message, webSocket);
        }
    }

    private static async Task<string?> ReceiveMessage(WebSocket webSocket)
    {
        var message = new StringBuilder();

        while (true)
        {
            var buffer = WebSocket.CreateServerBuffer(ReceiveBufferSize);
            var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (!receiveResult.CloseStatus.HasValue)
            {
                int termination = Array.IndexOf([.. buffer], (byte)0);
                if (termination == -1)
                {
                    message.Append(Encoding.UTF8.GetString([.. buffer], 0, buffer.Count));
                }
                else
                {
                    message.Append(Encoding.UTF8.GetString([.. buffer], 0, termination));
                }

                if (receiveResult.EndOfMessage)
                {
                    break;
                }
            }
            else
            {
                return null;
            }
        }


        return message.ToString();
    }
}
