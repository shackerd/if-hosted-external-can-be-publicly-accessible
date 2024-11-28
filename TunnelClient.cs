namespace wasm_http_reverse_proxy;

public class TunnelClient(WebSocketListener socketListener, string id)
    {
        public string Id { get; init; } = id;
        public WebSocketListener Listener { get; init; } = socketListener;
    }
