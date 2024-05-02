using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
    private const string ServerAddressDefault = "127.0.0.1";
    private const int ServerPortDefault = 8080;
    private const int SendBufferSize = 1024;
    private const int ReceiveBufferSize = 1024;

    public static async Task Main(string[] args)
    {
        string ServerAddress = args.Length > 0 ? args[0] : ServerAddressDefault;
        int ServerPort = args.Length > 1 ? int.Parse(args[1]) : ServerPortDefault;

        var webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri($"ws://{ServerAddress}:{ServerPort}"), CancellationToken.None);

        Console.WriteLine("Connected to server!");

        var buffer = WebSocket.CreateClientBuffer(ReceiveBufferSize, SendBufferSize);

        while (webSocket.State == WebSocketState.Open)
        {
            var message = Console.ReadLine();
            if (message == null || message.Length == 0)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
            }
            else
            {
                await SendMessage(webSocket, buffer, message);
            }
        }
    }

    private static async Task SendMessage(ClientWebSocket webSocket, ArraySegment<byte> buffer, string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        int messageLength = messageBytes.Length;
        int offset = 0;
        int count = SendBufferSize;
        while (offset < messageLength)
        {
            bool endOfMessage = offset + count < messageBytes.Length;
            count = Math.Min(count, messageLength - offset);
            new ArraySegment<byte>(messageBytes, offset, count).CopyTo(buffer);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
            offset += count;
        }
    }
}