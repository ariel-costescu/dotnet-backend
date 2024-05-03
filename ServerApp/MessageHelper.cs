namespace BackendServer;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class MessageHelper
{
    private const int SendBufferSize = 1024;
    private const int ReceiveBufferSize = 1024;
    
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static async Task SendMessage(WebSocket webSocket, string message)
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
                count = await SendPartialMessage(webSocket, messageBytes, messageLength, offset, count, endOfMessage);
                offset += count;
                endOfMessage = offset + count >= messageLength;
            }
            if (offset < messageLength)
            {
                await SendPartialMessage(webSocket, messageBytes, messageLength, offset, count, endOfMessage); ;
            }
        }
    }

    private static async Task<int> SendPartialMessage(WebSocket webSocket, byte[] messageBytes,
                int messageLength, int offset, int count, bool endOfMessage)
    {
        count = Math.Min(count, messageLength - offset);
        var buffer = new ArraySegment<byte>(messageBytes, offset, count);
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
        return count;
    }

    public static async Task<string?> ReceiveMessage(WebSocket webSocket)
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
