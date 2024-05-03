using System.Net.WebSockets;
using System.Text.Json;
using Serilog;

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
                Log.Information($"SendGift: request={payload.ToString()}");

                string? playerId = LoginHandler.getAuthenticatedUserForWebSocket(webSocket);
                if (playerId == null)
                {
                    Log.Error("Unauthenticated user!");
                }
                else
                {
                    string friendPlayerId = sendGiftRequest.FriendPlayerId;
                    var resourceType = sendGiftRequest.ResourceType;
                    var delta = sendGiftRequest.ResourceValue;

                    UpdateResourcesHandler.UpdateBalancesForGift(playerId, friendPlayerId, resourceType, delta);

                    WebSocket? friendWebSocket = LoginHandler.GetWebSocketForUser(friendPlayerId);
                    if (friendWebSocket != null)
                    {
                        var giftEvent = new GiftEvent
                        {
                            FriendPlayerId = sendGiftRequest.FriendPlayerId,
                            ResourceType = sendGiftRequest.ResourceType,
                            ResourceValue = sendGiftRequest.ResourceValue
                        };


                        await SendGiftEvent(giftEvent, friendWebSocket);
                    }
                }

            }
        }
    }

    private static async Task SendGiftEvent(GiftEvent giftEvent, WebSocket webSocket)
    {
        string message = JsonSerializer.Serialize(giftEvent, MessageHelper.Options);
        await MessageHelper.SendMessage(webSocket, message);
    }

}
