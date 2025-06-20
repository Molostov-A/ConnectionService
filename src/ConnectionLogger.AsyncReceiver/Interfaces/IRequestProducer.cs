﻿namespace ConnectionLogger.AsyncReceiver.Interfaces;

public interface IRequestProducer
{
    Task SendAsync(object obj, string correlationId, Dictionary<string, object> headers);

    Task SendAsync(string message, string correlationId, Dictionary<string, object> headers);
}
