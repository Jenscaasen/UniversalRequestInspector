using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using UniversalRequestInspector.Hubs;
using UniversalRequestInspector.Models;
using UniversalRequestInspector.Services;

namespace UniversalRequestInspector.Controllers;

[ApiController]
[Route("requestsink/{sinkId}")]
public class RequestSinkController : ControllerBase
{
    private readonly RequestStorageService _storageService;
    private readonly IHubContext<RequestNotificationHub> _hubContext;
    private readonly RequestForwardingService _forwardingService;

    public RequestSinkController(
        RequestStorageService storageService, 
        IHubContext<RequestNotificationHub> hubContext,
        RequestForwardingService forwardingService)
    {
        _storageService = storageService;
        _hubContext = hubContext;
        _forwardingService = forwardingService;
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [HttpOptions]
    [HttpHead]
    [Route("{**path}")]
    public async Task<IActionResult> CaptureRequest(string sinkId, string? path = null)
    {
        var sink = _storageService.GetSink(sinkId);
        if (sink == null || !sink.IsActive)
        {
            return NotFound($"Request sink '{sinkId}' not found or inactive");
        }

        // Read the request body
        string body = string.Empty;
        if (Request.ContentLength > 0)
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            body = await reader.ReadToEndAsync();
        }

        // Extract headers
        var headers = new Dictionary<string, string>();
        foreach (var header in Request.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value.ToArray());
        }

        // Extract query parameters
        var queryParams = new Dictionary<string, string>();
        foreach (var param in Request.Query)
        {
            queryParams[param.Key] = string.Join(", ", param.Value.ToArray());
        }

        // Get client IP
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            clientIp = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? clientIp;
        }

        // Create request info
        var requestInfo = new RequestInfo
        {
            Method = Request.Method,
            Path = $"/{path ?? string.Empty}",
            Headers = headers,
            Body = body,
            QueryParams = queryParams,
            ClientIp = clientIp,
            UserAgent = Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty,
            ContentType = Request.ContentType ?? string.Empty,
            ContentLength = Request.ContentLength ?? 0
        };

        // Store the request
        _storageService.AddRequestToSink(sinkId, requestInfo);

        // Forward the request if forwarding is enabled
        ResponseInfo? responseInfo = null;
        if (sink.IsForwardingEnabled)
        {
            responseInfo = await _forwardingService.ForwardRequestAsync(requestInfo, sink.ForwardUrl!);
        }

        // Notify connected clients via SignalR
        await _hubContext.Clients.Group($"sink_{sinkId}")
            .SendAsync("NewRequest", requestInfo);

        // Return the forwarded response or a simple response
        if (responseInfo != null)
        {
            // Return the forwarded response
            Response.StatusCode = responseInfo.StatusCode;
            
            // Set response headers
            foreach (var header in responseInfo.Headers)
            {
                if (CanSetResponseHeader(header.Key))
                {
                    Response.Headers[header.Key] = header.Value;
                }
            }

            return Content(responseInfo.Body, responseInfo.ContentType);
        }
        else
        {
            // Return a simple response
            return Ok(new
            {
                message = "Request captured successfully",
                timestamp = requestInfo.Timestamp,
                requestId = requestInfo.Id,
                sink = sinkId
            });
        }
    }

    private static bool CanSetResponseHeader(string headerName)
    {
        // Skip headers that ASP.NET Core manages automatically
        var skipHeaders = new[]
        {
            "server", "date", "content-length", "transfer-encoding"
        };
        
        return !skipHeaders.Contains(headerName.ToLowerInvariant());
    }

    [HttpPost("api/forward-url")]
    public IActionResult SetForwardUrl(string sinkId, [FromBody] SetForwardUrlRequest request)
    {
        var sink = _storageService.GetSink(sinkId);
        if (sink == null || !sink.IsActive)
        {
            return NotFound($"Request sink '{sinkId}' not found or inactive");
        }

        var success = _storageService.SetForwardUrl(sinkId, request.ForwardUrl);
        if (success)
        {
            return Ok(new { message = "Forward URL updated successfully", forwardUrl = request.ForwardUrl });
        }

        return BadRequest("Failed to update forward URL");
    }
}

public class SetForwardUrlRequest
{
    public string? ForwardUrl { get; set; }
}