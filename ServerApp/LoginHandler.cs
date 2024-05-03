using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace BackendServer;
public class LoginHandler
{
    private static readonly ConcurrentDictionary<string, string> deviceIdToPlayerId = new();
    private static readonly ConcurrentDictionary<WebSocket, string> webSocketToPlayerId = new();

    public static async Task HandleLogin(object? payload, WebSocket webSocket)
    {
        if (payload != null)
        {
            var loginRequest = JsonSerializer.Deserialize<LoginRequest>(payload.ToString() ?? "");

            if (loginRequest != null)
            {
                Console.WriteLine($"Login: request={payload.ToString()}");

                if (loginRequest.DeviceId != null)
                {
                    var loginResponse = new LoginResponse();
                    string deviceId = loginRequest.DeviceId;
                    if (deviceIdToPlayerId.TryGetValue(deviceId, out var playerId))
                    {
                        loginResponse.Error = $"User {playerId} already logged in!";
                        Console.WriteLine(loginResponse.Error);
                    }
                    else
                    {
                        playerId = Guid.NewGuid().ToString();
                        deviceIdToPlayerId.TryAdd(deviceId, playerId);
                        loginResponse.PlayerId = playerId;
                        authenticateUser(webSocket, playerId);
                    }
                    

                    await SendLoginResponse(loginResponse, webSocket);
                }
            }
        }
    }

    private static void authenticateUser(WebSocket webSocket, string playerId)
    {
        webSocketToPlayerId.AddOrUpdate(webSocket, playerId, (k, v) => playerId);
    }

    public static string? getAuthenticatedUserForWebSocket(WebSocket webSocket)
    {
        webSocketToPlayerId.TryGetValue(webSocket, out var playerId);
        return playerId;
    }

    private static async Task SendLoginResponse(LoginResponse loginResponse, WebSocket webSocket)
    {
        string message = JsonSerializer.Serialize(loginResponse);
        await MessageHelper.SendMessage(webSocket, message);
    }
}
