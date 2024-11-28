using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations.Rules;

namespace wasm_http_reverse_proxy;
public class WebSocketController : ControllerBase
{
    private readonly TunnelClientRepository _repository;

    public WebSocketController(TunnelClientRepository repository)
    {
        _repository = repository;
    }

    [Route("tunnel")]
    [HttpPost]
    public async Task<IActionResult> Tunnel()
    {
        // Compute a unique name/identifier for the websocket client
        string value = UrlEncoder.Default.Encode(Convert.ToBase64String(
            SHA512.HashData(
                Encoding.UTF8.GetBytes($"{HttpContext.Connection.RemoteIpAddress}_{HttpContext.Connection.RemotePort}")
            )
        ));

        // We created nothing, but response format is appropriate
        return Created($"/ws/{value}", null);
    }

    [Route("/ws/{name}")]
    [Swashbuckle.AspNetCore.Annotations.SwaggerIgnore]
    public async Task Get(string name)
    {

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var client = await TunnelClientFactory.CreateAsync(HttpContext, name, CancellationToken.None);
            _repository.Add(client);

            // Keep the websocket alive, but i'm not sure if this is the best way to
            await client.Listener.KeepAlive();

            // You know what, out of this scope, the websocket is closed, so we remove it from the repository
            _repository.Remove(client.Id);

        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
