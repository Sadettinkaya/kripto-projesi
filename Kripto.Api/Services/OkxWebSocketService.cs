using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Kripto.Api.Services
{
    public class OkxWebSocketService
    {
        private readonly ClientWebSocket _ws = new();
        private readonly Uri _okxUri = new("wss://ws.okx.com:8443/ws/v5/public");
        private readonly List<string> _symbols;
        private readonly Dictionary<string, decimal> _prices = new();

        public OkxWebSocketService(List<string> symbols)
        {
            _symbols = symbols;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _ws.ConnectAsync(_okxUri, cancellationToken);

            // OKX WebSocket subscribe mesajı
            var subscribeMsg = new
            {
                op = "subscribe",
                args = _symbols.Select(s => new { channel = "tickers", instId = s }).ToArray()
            };
            var msg = JsonSerializer.Serialize(subscribeMsg);
            await _ws.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, cancellationToken);

            // Mesajları dinle
            var buffer = new byte[4096];
            while (_ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in dataArr.EnumerateArray())
                        {
                            var symbol = item.GetProperty("instId").GetString();
                            var price = decimal.Parse(item.GetProperty("last").GetString() ?? "0");
                            _prices[symbol!] = price;
                        }
                    }
                }
            }
        }

        public Dictionary<string, decimal> GetPrices() => new(_prices);
    }
}