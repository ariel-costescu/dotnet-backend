using System.Net.WebSockets;
using System.Text.Json;

namespace BackendServer;

public class UpdateResourcesHandler
{
    private static readonly Dictionary<string, BalancePerResourceType> balancesPerUser = [];
    private static readonly object syncLock = new();

    public static async Task HandleUpdateResources(object? payload, WebSocket webSocket)
    {
        if (payload != null)
        {

            var updateResourcesRequest = JsonSerializer.Deserialize<UpdateResourcesRequest>(payload.ToString() ?? "",
                    MessageHelper.Options);

            if (updateResourcesRequest != null)
            {
                Console.WriteLine($"UpdateResources: request={payload.ToString()}");

                var updateResourcesResponse = new UpdateResourcesResponse();

                string? playerId = LoginHandler.getAuthenticatedUserForWebSocket(webSocket);
                if (playerId == null)
                {
                    updateResourcesResponse.Error = "Unauthenticated user!";
                    Console.WriteLine(updateResourcesResponse.Error);
                }
                else
                {
                    lock (syncLock)
                    {
                        var resourceType = updateResourcesRequest.ResourceType;
                        var delta = updateResourcesRequest.ResourceValue;
                        updateResourcesResponse.ResourceType = resourceType;
                        balancesPerUser.TryGetValue(playerId, out var balances);
                        if (balances == null)
                        {
                            balances = new BalancePerResourceType();
                            balancesPerUser[playerId] = balances;
                        }
                        balances.UpdateBalance(resourceType, delta);
                        updateResourcesResponse.Balance = balances.GetBalanceForResourceType(resourceType) ?? 0;
                    }
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

class BalancePerResourceType
{
    private readonly Dictionary<ResourceTypeEnum, long> balances = [];

    public BalancePerResourceType()
    {
        foreach(ResourceTypeEnum resourceType in Enum.GetValues<ResourceTypeEnum>())
        {
            balances[resourceType] = 0;
        }
    }

    public void UpdateBalance(ResourceTypeEnum resourceType, long delta)
    {
        balances[resourceType] += delta;
    }

    public long? GetBalanceForResourceType(ResourceTypeEnum resourceType)
    {
        return balances[resourceType];
    }
}
