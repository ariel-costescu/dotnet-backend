using System.Net.WebSockets;
using System.Text.Json;
using Serilog;

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
                Log.Information($"UpdateResources: request={payload.ToString()}");

                var updateResourcesResponse = new UpdateResourcesResponse();

                string? playerId = LoginHandler.getAuthenticatedUserForWebSocket(webSocket);
                if (playerId == null)
                {
                    updateResourcesResponse.Error = "Unauthenticated user!";
                    Log.Error(updateResourcesResponse.Error);
                }
                else
                {
                    var resourceType = updateResourcesRequest.ResourceType;
                    var delta = updateResourcesRequest.ResourceValue;
                    var newBalance = UpdateBalanceForUser(playerId, resourceType, delta);
                    updateResourcesResponse.ResourceType = resourceType;
                    updateResourcesResponse.Balance = newBalance;

                };

                await SendUpdateResourcesResponse(updateResourcesResponse, webSocket);
            }
        }
    }

    public static long UpdateBalanceForUser(string playerId, ResourceTypeEnum resourceType, long delta)
    {
        lock (syncLock)
        {
            return UpdateBalance(playerId, resourceType, delta);
        }
    }

    private static long UpdateBalance(string playerId, ResourceTypeEnum resourceType, long delta)
    {
        balancesPerUser.TryGetValue(playerId, out var balances);
        if (balances == null)
        {
            balances = new BalancePerResourceType();
            balancesPerUser[playerId] = balances;
        }
        return balances.UpdateBalance(resourceType, delta);
    }

    public static void UpdateBalancesForGift(string fromPlayerId, string toPlayerId,
        ResourceTypeEnum resourceType, long delta)
    {
        lock (syncLock)
        {
            UpdateBalance(fromPlayerId, resourceType, -delta);
            UpdateBalance(toPlayerId, resourceType, delta);
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
        foreach (ResourceTypeEnum resourceType in Enum.GetValues<ResourceTypeEnum>())
        {
            balances[resourceType] = 0;
        }
    }

    public long UpdateBalance(ResourceTypeEnum resourceType, long delta)
    {
        balances[resourceType] += delta;
        return balances[resourceType];
    }

    public long? GetBalanceForResourceType(ResourceTypeEnum resourceType)
    {
        return balances[resourceType];
    }
}
