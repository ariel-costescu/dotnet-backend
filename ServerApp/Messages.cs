namespace BackendServer;

public class Message
{
    public string? Type { get; set; }
    public object? Payload { get; set; }
}

public class LoginRequest
{
    public string? DeviceId { get; set; }
}

public class LoginResponse
{
    public string? PlayerId { get; set; }
    public string? Error { get; set; }
}
public enum ResourceTypeEnum{ Coins, Rolls }

public class UpdateResourcesRequest
{
    public ResourceTypeEnum ResourceType { get; set; }
    public long ResourceValue { get; set; }
}

public class UpdateResourcesResponse
{
    public ResourceTypeEnum? ResourceType { get; set; }
    public long? Balance { get; set; }
    public string? Error { get; set; }
}

public class SendGiftRequest
{
    public required string FriendPlayerId { get; set; }
    public ResourceTypeEnum ResourceType { get; set; }
    public long ResourceValue { get; set; }
}

public class GiftEvent
{
    public required string FriendPlayerId { get; set; }
    public ResourceTypeEnum ResourceType { get; set; }
    public long ResourceValue { get; set; }
}