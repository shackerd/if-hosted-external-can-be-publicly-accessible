using System.Net.WebSockets;

namespace wasm_http_reverse_proxy;

public class WebSocketListener
    {
        private readonly WebSocket _webSocket;
        private readonly CancellationToken _cancellationToken;
        private readonly Queue<ArraySegment<byte>> _queue = new();

        public WebSocket WebSocket => _webSocket;

        public WebSocketListener(WebSocket webSocket, CancellationToken cancellationToken = default)
        {
            _webSocket = webSocket;
            _cancellationToken = cancellationToken;
        }

        public async Task KeepAlive()
        {
            do
            {
                await Task.Delay(1000);

            } while (!_cancellationToken.IsCancellationRequested);
        }

        public Queue<ArraySegment<byte>> GetQueue()
        {
            return _queue;
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType type)
        {
            await _webSocket.SendAsync(buffer, type, true, _cancellationToken);
        }
    }
