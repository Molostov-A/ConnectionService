using ConnectionLogger.Messaging.Messages;
using System.Collections.Concurrent;

namespace ConnectionLogger.AsyncReceiver;
public class ResponsePool
{
    private readonly ConcurrentDictionary<string, ResponseResult> _responses = new();

    public void AddResponse(string correlationId, ResponseResult response)
    {
        _responses[correlationId] = response;
    }

    public ResponseResult GetResponse(string correlationId)
    {
        _responses.TryGetValue(correlationId, out var response);
        return response;
    }
}
