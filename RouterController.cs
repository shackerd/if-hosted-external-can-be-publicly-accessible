using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
namespace wasm_http_reverse_proxy;

public class RouterController : ControllerBase
{
    private readonly TunnelClientRepository _repository;

    public RouterController(TunnelClientRepository repository)
    {
        _repository = repository;
    }

    [Route("/route/{name}")]
    [HttpGet]
    public async Task Route(string name)
    {
        StringBuilder builder = new();

        // This is a demo, so this is just a dummy value to send to the websocket client
        builder.Append($"{Request.Method} {Request.Path.ToString().Replace(name, string.Empty)} {Request.Headers.UserAgent}");

        var client = _repository.GetClients().FirstOrDefault(c => c.Id == name);

        // early return stuff & cleanup
        if (client == null || client.Listener.WebSocket.State != WebSocketState.Open)
        {
            Response.StatusCode = (int)HttpStatusCode.BadGateway;
            return;
        }

        if (client.Listener.WebSocket.State != WebSocketState.Open)
        {
            _repository.Remove(client.Id);
            Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
        }

        // Send the request to the websocket client
        await client.Listener.SendAsync(Encoding.UTF8.GetBytes(builder.ToString()), WebSocketMessageType.Text);

        // Receive the response from the websocket client
        ArraySegment<byte> buffer = new (new byte[1024]);
        var res = await client.Listener.WebSocket.ReceiveAsync(buffer, HttpContext.RequestAborted);

        // This should not be here, websocket must be closed elsewhere to avoid client websocket retention on gracefull close packet
        if (res.MessageType == WebSocketMessageType.Close)
        {
            _repository.Remove(client.Id);
            await client.Listener.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", default);
            Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
            return;
        }

        // Deserialize the response from the websocket client and write to connected http client
        string json = Encoding.UTF8.GetString(buffer!.Array!, 0, res.Count);
        var response = JsonSerializer.Deserialize<JsResponse>(json);

        Response.StatusCode = response!.StatusCode;

        // this kind of glue stuff..
        foreach (var header in response.Headers)
        {
            Response.Headers.Append(header.Key, header.Value);
        }

        var newBuffer = Encoding.UTF8.GetBytes(response.Body);

        await Response.Body.WriteAsync(newBuffer, 0, newBuffer.Length, HttpContext.RequestAborted);
    }

    // I know this not a complete response object, but i'm lazy to write it all, it's just a demo
    // Shh, i can hear you trashing me already, don't worry i would not write it that way in a real project
    public class JsResponse
    {
        public int StatusCode { get; set; }
        public  KeyValuePair<string, string>[] Headers { get; set; }
        public string Body { get; set; }
    }
}