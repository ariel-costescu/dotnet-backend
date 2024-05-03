using System.Net.WebSockets;
using System.Text.Json;

namespace BackendServer;
public class LoginHandler
{

    public static async Task HandleLogin(object? payload, WebSocket webSocket)
    {
        if (payload != null)
        {
            var loginRequest = JsonSerializer.Deserialize<LoginRequest>(payload.ToString() ?? "");

            if (loginRequest != null)
            {
                Console.WriteLine($"Login: request={payload.ToString()}");

                // TODO: Implement
                var loginResponse = new LoginResponse
                {
                    PlayerId = loginRequest.DeviceId
                };

                await SendLoginResponse(loginResponse, webSocket);
            }
        }
    }

    private static async Task SendLoginResponse(LoginResponse loginResponse, WebSocket webSocket)
    {
        string message = JsonSerializer.Serialize(loginResponse);
        await MessageHelper.SendMessage(webSocket, message);
    }
}
