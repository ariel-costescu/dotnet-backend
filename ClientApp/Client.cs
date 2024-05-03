using System.Net.WebSockets;
using System.Text;

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

        while (webSocket.State == WebSocketState.Open)
        {
            var message = Console.ReadLine();
            if (message == null || message.Length == 0)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
            }
            else
            {
                await SendMessage(webSocket, message);
            }
        }
    }

    private static async Task SendMessage(ClientWebSocket webSocket, string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        int messageLength = messageBytes.Length;

        if (messageLength <= SendBufferSize)
        {
            var buffer = new ArraySegment<byte>(messageBytes, 0, messageLength);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        }
        else
        {
            int offset = 0;
            int count = SendBufferSize;
            bool endOfMessage = offset + count >= messageLength;
            while (!endOfMessage)
            {
                count = await SendPartialMessage(webSocket, messageBytes, messageLength, offset, count, endOfMessage); offset += count;
                endOfMessage = offset + count >= messageLength;
            }
            if (offset < messageLength)
            {
                count = await SendPartialMessage(webSocket, messageBytes, messageLength, offset, count, endOfMessage); offset += count;
            }
        }
    }

    private static async Task<int> SendPartialMessage(ClientWebSocket webSocket, byte[] messageBytes,
                int messageLength, int offset, int count, bool endOfMessage)
    {
        count = Math.Min(count, messageLength - offset);
        var buffer = new ArraySegment<byte>(messageBytes, offset, count);
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
        return count;
    }
}