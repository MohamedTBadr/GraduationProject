using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

public class SseConnectionManager
{
    // userId → response stream
    private readonly ConcurrentDictionary<Guid, HttpResponse> _connections = new();

    public void Add(Guid userId, HttpResponse response) =>
        _connections[userId] = response;

    public void Remove(Guid userId) =>
        _connections.TryRemove(userId, out _);

    public bool TryGet(Guid userId, out HttpResponse? response)
    {
        if (_connections.TryGetValue(userId, out var r))
        {
            response = r;
            return true;
        }
        response = null;
        return false;
    }
}