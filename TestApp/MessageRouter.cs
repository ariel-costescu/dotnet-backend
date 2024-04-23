using System.Net.Sockets;

public class MessageRouter : IMessageRouter
{
    private readonly Dictionary<string, Func<string, NetworkStream, Task>> _handlers;

    public MessageRouter()
    {
        _handlers = new Dictionary<string, Func<string, NetworkStream, Task>>();
    }

    public void RegisterHandler(string messageType, Func<string, NetworkStream, Task> handler)
    {
        _handlers.Add(messageType, handler);
    }

    public async Task RouteMessage(string message, NetworkStream stream)
    {
        // Extract message type from the message format
        var messageType = GetMessageType(message);

        if (_handlers.TryGetValue(messageType, out var handler))
        {
            await handler(message, stream);
        }
        else
        {
            Console.WriteLine($"Unhandled message type: {messageType}");
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
    void RegisterHandler(string messageType, Func<string, NetworkStream, Task> handler);
    Task RouteMessage(string message, NetworkStream stream);
}
