namespace wasm_http_reverse_proxy;

// This is a simple repository to store the websocket clients, no concurrency control, no nothing
public class TunnelClientRepository
    {
        private readonly List<TunnelClient> _clients = [];

        public void Add(TunnelClient client)
        {
            _clients.Add(client);
        }

        public void Remove(string id)
        {
            var client = _clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                _clients.Remove(client);
            }
        }

        public TunnelClient? Get(string id)
        {
            return _clients.FirstOrDefault(c => c.Id == id);
        }

        public List<TunnelClient> GetClients()
        {
            return _clients;
        }
    }
