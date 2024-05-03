namespace ClientApp;

using System.Net.WebSockets;
using System.Text.Json;
using BackendServer;

public class Client
{
    private const string ServerAddressDefault = "127.0.0.1";
    private const int ServerPortDefault = 8080;

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
                Message? jsonMessage = null;
                try
                {
                    jsonMessage = JsonSerializer.Deserialize<Message>(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to parse json message due to exception: {ex.Message}");
                }

                if (jsonMessage != null)
                {
                    var messageType = jsonMessage.Type ?? "default";

                    switch (messageType)
                    {
                        case "Login":
                            await HandleLoginAsync(webSocket, message);
                            break;
                        case "UpdateResources":
                            await HandleUpdateResourcesAsync(webSocket, message);
                            break;
                        case "SendGift":
                            await HandleSendGiftAsync(webSocket, message);
                            break;
                        default:
                            Console.WriteLine($"Unhandled message type: {messageType}");
                            break;
                    }

                }
            }
        }
    }

    private static async Task HandleLoginAsync(ClientWebSocket webSocket, string message)
    {
        await MessageHelper.SendMessage(webSocket, message);
        if (webSocket.State == WebSocketState.Open)
        {
            var response = await MessageHelper.ReceiveMessage(webSocket);
            if (response != null)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(response);
                Console.WriteLine($"Login response: PlayerId={loginResponse?.PlayerId}");
            }
        }
    }

    private static async Task HandleUpdateResourcesAsync(ClientWebSocket webSocket, string message)
    {
        await MessageHelper.SendMessage(webSocket, message);
        if (webSocket.State == WebSocketState.Open)
        {
            var response = await MessageHelper.ReceiveMessage(webSocket);
            if (response != null)
            {
                var updateResourcesResponse = JsonSerializer.Deserialize<UpdateResourcesResponse>(response, MessageHelper.Options);
                Console.WriteLine($"UpdateResources response: Balance={response}");
            }
        }
    }

    private static async Task HandleSendGiftAsync(ClientWebSocket webSocket, string message)
    {
        await MessageHelper.SendMessage(webSocket, message);
        if (webSocket.State == WebSocketState.Open)
        {
            var response = await MessageHelper.ReceiveMessage(webSocket);
            if (response != null)
            {
                var giftEvent = JsonSerializer.Deserialize<GiftEvent>(response, MessageHelper.Options);
                Console.WriteLine($"SendGiftRequest response: giftEvent={response}");
            }
        }
    }
}