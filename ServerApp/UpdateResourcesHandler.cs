using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendServer;

public class UpdateResourcesHandler
{

    public static async Task HandleUpdateResources(object? payload, WebSocket webSocket)
    {
        if (payload != null)
        {

            var updateResourcesRequest = JsonSerializer.Deserialize<UpdateResourcesRequest>(payload.ToString() ?? "",
                    MessageHelper.Options);

            if (updateResourcesRequest != null)
            {
                Console.WriteLine($"UpdateResources: request={payload.ToString()}");

                // TODO: Implement
                var updateResourcesResponse = new UpdateResourcesResponse
                {
                    ResourceType = updateResourcesRequest.ResourceType,
                    Balance = updateResourcesRequest.ResourceValue
                };

                await SendUpdateResourcesResponse(updateResourcesResponse, webSocket);
            }
        }
    }

    private static async Task SendUpdateResourcesResponse(UpdateResourcesResponse updateResourcesResponse, WebSocket webSocket)
    {
        string message = JsonSerializer.Serialize(updateResourcesResponse, MessageHelper.Options);
        await MessageHelper.SendMessage(webSocket, message);
    }
}
