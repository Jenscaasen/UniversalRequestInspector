namespace UniversalRequestInspector.Models;

public class RequestInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    
    // Response information
    public ResponseInfo? Response { get; set; }
    public bool WasForwarded { get; set; }
    public string? ForwardedTo { get; set; }
    public TimeSpan? ForwardDuration { get; set; }
}

public class ResponseInfo
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}