using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendServer;

public class SendGiftHandler
{

    public static async Task HandleSendGift(object? payload, WebSocket webSocket)
    {
        if (payload != null)
        {

            var sendGiftRequest = JsonSerializer.Deserialize<SendGiftRequest>(payload.ToString() ?? "",
                    MessageHelper.Options);

            if (sendGiftRequest != null)
            {
                Console.WriteLine($"SendGift: request={payload.ToString()}");

                // TODO: Implement
                var giftEvent = new GiftEvent
                {
                    FriendPlayerId = sendGiftRequest.FriendPlayerId,
                    ResourceType = sendGiftRequest.ResourceType,
                    ResourceValue = sendGiftRequest.ResourceValue
                };

                await SendGiftEvent(giftEvent, webSocket);
            }
        }
    }

    private static async Task SendGiftEvent(GiftEvent giftEvent, WebSocket webSocket)
    {
        string message = JsonSerializer.Serialize(giftEvent, MessageHelper.Options);
        await MessageHelper.SendMessage(webSocket, message);
    }

}
