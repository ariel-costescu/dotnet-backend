namespace BackendServer;

using System.Net.WebSockets;
using System.Text.Json;

public class MessageRouter : IMessageRouter
{
    private readonly Dictionary<string, Func<object?, WebSocket, Task>> _handlers;

    public MessageRouter()
    {
        _handlers = [];
    }

    public void RegisterHandler(string messageType, Func<object?, WebSocket, Task> handler)
    {
        _handlers.Add(messageType, handler);
    }

    public async Task RouteMessage(string message, WebSocket webSocket)
    {
        Message? jsonMessage = null;
        try
        {
            jsonMessage = JsonSerializer.Deserialize<Message>(message);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Unable to parse json message due to exception: {ex.Message}");
        }

        if (jsonMessage != null)
        {
            var messageType = jsonMessage.Type ?? "default";

            if (_handlers.TryGetValue(messageType, out var handler))
            {
                await handler(jsonMessage.Payload, webSocket);
            }
            else
            {
                Console.WriteLine($"Unhandled message type: {messageType}");
                Console.WriteLine($"Message was : \"{message}\"");
            }
        }
    }
}

public interface IMessageRouter
{
    void RegisterHandler(string messageType, Func<object?, WebSocket, Task> handler);
    Task RouteMessage(string message, WebSocket webSocket);
}
