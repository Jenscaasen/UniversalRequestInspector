using Microsoft.AspNetCore.SignalR;
using UniversalRequestInspector.Services;

namespace UniversalRequestInspector.Hubs;

public class RequestNotificationHub : Hub
{
    private readonly RequestStorageService _storageService;

    public RequestNotificationHub(RequestStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task JoinSink(string sinkId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sink_{sinkId}");
        
        // Update the sink with the current connection ID
        var sink = _storageService.GetSink(sinkId);
        if (sink != null)
        {
            sink.ConnectionId = Context.ConnectionId;
        }
    }

    public async Task LeaveSink(string sinkId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sink_{sinkId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any sinks associated with this connection
        var activeSinks = _storageService.GetActiveSinks()
            .Where(s => s.ConnectionId == Context.ConnectionId)
            .ToList();

        foreach (var sink in activeSinks)
        {
            sink.IsActive = false;
        }

        await base.OnDisconnectedAsync(exception);
    }
}