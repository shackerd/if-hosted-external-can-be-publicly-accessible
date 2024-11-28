namespace wasm_http_reverse_proxy;

public class TunnelClientFactory
    {
        public async static Task<TunnelClient> CreateAsync(HttpContext httpContext, string name, CancellationToken cancellationToken = default)
        {
            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            WebSocketListener listener = new (webSocket, cancellationToken);
            return new TunnelClient(listener, name);
        }
    }
