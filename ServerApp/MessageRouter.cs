using System.Net.WebSockets;

public class MessageRouter : IMessageRouter
{
    private readonly Dictionary<string, Func<string, WebSocket, Task>> _handlers;

    public MessageRouter()
    {
        _handlers = new Dictionary<string, Func<string, WebSocket, Task>>();
    }

    public void RegisterHandler(string messageType, Func<string, WebSocket, Task> handler)
    {
        _handlers.Add(messageType, handler);
    }

    public async Task RouteMessage(string message, WebSocket webSocket)
    {
        // Extract message type from the message format
        var messageType = GetMessageType(message);

        if (_handlers.TryGetValue(messageType, out var handler))
        {
            await handler(message, webSocket);
        }
        else
        {
            Console.WriteLine($"Unhandled message type: {messageType}");
            Console.WriteLine($"Message was : \"{message}\"");
        }
    }

    private string GetMessageType(string message)
    {
        // Implement logic to extract the message type from your message format
        return "default"; // Replace with actual logic
    }
}

public interface IMessageRouter
{
    void RegisterHandler(string messageType, Func<string, WebSocket, Task> handler);
    Task RouteMessage(string message, WebSocket webSocket);
}
