namespace UniversalRequestInspector.Models;

public class RequestSink
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public List<RequestInfo> Requests { get; set; } = new();
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int MaxRequests { get; set; } = 100;
    
    // Forward URL configuration
    public string? ForwardUrl { get; set; }
    public bool IsForwardingEnabled => !string.IsNullOrEmpty(ForwardUrl);
    
    public void AddRequest(RequestInfo request)
    {
        LastActivity = DateTime.UtcNow;
        Requests.Add(request);
        
        // Keep only the latest requests if we exceed the limit
        if (Requests.Count > MaxRequests)
        {
            Requests.RemoveAt(0);
        }
    }
    
    public bool IsExpired(TimeSpan timeout)
    {
        return DateTime.UtcNow - LastActivity > timeout;
    }
}