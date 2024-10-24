using System.Net.WebSockets;
using System.Text;

namespace EquipmentStateManagement.Services
    {
    public static class WebSocketHandler
        {
        private static List<WebSocket> _sockets = new List<WebSocket>();

        public static async Task HandleWebSocketAsync(WebSocket webSocket)
            {
            _sockets.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
                {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                await BroadcastMessageAsync(message);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _sockets.Remove(webSocket);
            }

        public static async Task BroadcastMessageAsync(string message)
            {
            var buffer = Encoding.UTF8.GetBytes(message);
            foreach (var socket in _sockets)
                {
                if (socket.State == WebSocketState.Open)
                    {
                    await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }

        public static async Task SendMessageAsync(WebSocket socket, string message)
            {
            var buffer = Encoding.UTF8.GetBytes(message);
            if (socket.State == WebSocketState.Open)
                {
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }