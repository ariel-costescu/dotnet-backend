using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Server
{
    private static readonly IMessageRouter _messageRouter = new MessageRouter();

    public static async Task Main(string[] args)
    {
        var listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();

        Console.WriteLine("Server listening on port 8080...");

        while (true)
        {
            var clientSocket = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected!");

            await HandleClient(clientSocket);
        }
    }

    private static async Task HandleClient(TcpClient clientSocket)
    {
        using (var networkStream = clientSocket.GetStream())
        {
            if (!ValidateHandshake(networkStream))
            {
                Console.WriteLine("Invalid WebSocket handshake");
                return;
            }

            var message = await ReceiveMessage(networkStream);
            while (message != null)
            {
                await _messageRouter.RouteMessage(message, networkStream);
                message = await ReceiveMessage(networkStream);
            }
        }
    }

    private static bool ValidateHandshake(NetworkStream stream)
    {
        // Implement basic WebSocket handshake validation logic here
        // (check for Upgrade header, etc.)
        return true; // Replace with actual validation
    }

    private static async Task<string> ReceiveMessage(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var message = new StringBuilder();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            // Implement logic to handle fragmented messages and masking (if needed)
            message.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        return message.ToString();
    }
}
