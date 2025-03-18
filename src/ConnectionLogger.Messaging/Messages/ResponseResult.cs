namespace ConnectionLogger.Messaging.Messages;

public class ResponseResult
{
    public required string Message { get; set; }

    public object? Result { get; set; }

    public bool Success { get; set; }
}
