using System.Diagnostics;
using System.Text;
using UniversalRequestInspector.Models;

namespace UniversalRequestInspector.Services;

public class RequestForwardingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RequestForwardingService> _logger;

    public RequestForwardingService(HttpClient httpClient, ILogger<RequestForwardingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResponseInfo?> ForwardRequestAsync(RequestInfo requestInfo, string forwardUrl)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Construct the full URL
            var targetUrl = CombineUrls(forwardUrl, requestInfo.Path);
            
            // Add query parameters
            if (requestInfo.QueryParams.Any())
            {
                var queryString = string.Join("&", requestInfo.QueryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                targetUrl += (targetUrl.Contains('?') ? "&" : "?") + queryString;
            }

            // Create the HTTP request
            using var httpRequest = new HttpRequestMessage(new HttpMethod(requestInfo.Method), targetUrl);

            // Add headers (excluding problematic ones)
            foreach (var header in requestInfo.Headers)
            {
                if (ShouldForwardHeader(header.Key))
                {
                    try
                    {
                        if (IsContentHeader(header.Key))
                        {
                            // Content headers will be added with the content
                            continue;
                        }
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to add header {HeaderName}: {Error}", header.Key, ex.Message);
                    }
                }
            }

            // Add request body for methods that support it
            if (!string.IsNullOrEmpty(requestInfo.Body) && MethodSupportsBody(requestInfo.Method))
            {
                httpRequest.Content = new StringContent(requestInfo.Body, Encoding.UTF8);
                
                // Set content type if available
                if (!string.IsNullOrEmpty(requestInfo.ContentType))
                {
                    httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(requestInfo.ContentType);
                }
            }

            // Send the request
            using var response = await _httpClient.SendAsync(httpRequest);
            stopwatch.Stop();

            // Read response
            var responseBody = await response.Content.ReadAsStringAsync();
            
            // Extract response headers
            var responseHeaders = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            }
            foreach (var header in response.Content.Headers)
            {
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            }

            var responseInfo = new ResponseInfo
            {
                StatusCode = (int)response.StatusCode,
                StatusText = response.ReasonPhrase ?? string.Empty,
                Headers = responseHeaders,
                Body = responseBody,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty,
                ContentLength = response.Content.Headers.ContentLength ?? responseBody.Length
            };

            // Update request info
            requestInfo.WasForwarded = true;
            requestInfo.ForwardedTo = targetUrl;
            requestInfo.ForwardDuration = stopwatch.Elapsed;
            requestInfo.Response = responseInfo;

            return responseInfo;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to forward request to {ForwardUrl}", forwardUrl);
            
            // Create error response
            var errorResponse = new ResponseInfo
            {
                StatusCode = 502,
                StatusText = "Bad Gateway - Forward Failed",
                Headers = new Dictionary<string, string>(),
                Body = $"Failed to forward request: {ex.Message}",
                ContentType = "text/plain"
            };

            requestInfo.WasForwarded = true;
            requestInfo.ForwardedTo = forwardUrl;
            requestInfo.ForwardDuration = stopwatch.Elapsed;
            requestInfo.Response = errorResponse;

            return errorResponse;
        }
    }

    private static string CombineUrls(string baseUrl, string path)
    {
        baseUrl = baseUrl.TrimEnd('/');
        path = path.TrimStart('/');
        return string.IsNullOrEmpty(path) ? baseUrl : $"{baseUrl}/{path}";
    }

    private static bool ShouldForwardHeader(string headerName)
    {
        // Skip headers that shouldn't be forwarded
        var skipHeaders = new[]
        {
            "host", "connection", "transfer-encoding", "upgrade",
            "proxy-connection", "proxy-authenticate", "proxy-authorization"
        };
        
        return !skipHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static bool IsContentHeader(string headerName)
    {
        var contentHeaders = new[]
        {
            "content-type", "content-length", "content-encoding",
            "content-language", "content-location", "content-md5",
            "content-range", "expires", "last-modified"
        };
        
        return contentHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static bool MethodSupportsBody(string method)
    {
        var methodsWithBody = new[] { "POST", "PUT", "PATCH", "DELETE" };
        return methodsWithBody.Contains(method.ToUpperInvariant());
    }
}