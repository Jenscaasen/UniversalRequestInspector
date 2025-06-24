using System.Collections.Concurrent;
using UniversalRequestInspector.Models;

namespace UniversalRequestInspector.Services;

public class RequestStorageService
{
    private readonly ConcurrentDictionary<string, RequestSink> _sinks = new();
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _sinkTimeout = TimeSpan.FromHours(1);

    public RequestStorageService()
    {
        // Run cleanup every 10 minutes
        _cleanupTimer = new Timer(CleanupExpiredSinks, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }

    public string CreateSink(string connectionId)
    {
        var sinkId = GenerateShortId();
        var sink = new RequestSink
        {
            Id = sinkId,
            ConnectionId = connectionId
        };
        
        _sinks.TryAdd(sinkId, sink);
        return sinkId;
    }

    public RequestSink? GetSink(string sinkId)
    {
        _sinks.TryGetValue(sinkId, out var sink);
        return sink;
    }

    public void AddRequestToSink(string sinkId, RequestInfo request)
    {
        if (_sinks.TryGetValue(sinkId, out var sink))
        {
            sink.AddRequest(request);
        }
    }

    public bool RemoveSink(string sinkId)
    {
        return _sinks.TryRemove(sinkId, out _);
    }

    public bool SetForwardUrl(string sinkId, string? forwardUrl)
    {
        if (_sinks.TryGetValue(sinkId, out var sink))
        {
            sink.ForwardUrl = forwardUrl;
            return true;
        }
        return false;
    }

    public IEnumerable<RequestSink> GetActiveSinks()
    {
        return _sinks.Values.Where(s => s.IsActive);
    }

    private void CleanupExpiredSinks(object? state)
    {
        var expiredSinks = _sinks.Values
            .Where(sink => sink.IsExpired(_sinkTimeout))
            .ToList();

        foreach (var sink in expiredSinks)
        {
            _sinks.TryRemove(sink.Id, out _);
        }
    }

    private static string GenerateShortId()
    {
        // Generate a short, URL-friendly ID
        var guid = Guid.NewGuid().ToString("N");
        return guid[..12]; // Take first 12 characters
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}