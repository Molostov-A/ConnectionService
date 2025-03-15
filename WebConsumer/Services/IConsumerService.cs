using MessageBrokerModelsLibrary.Models;
using MessageBrokerToolkit.Interfaces;

namespace WebConsumer.Services;

public interface IConsumerService: IConsumerServiceMBT
{
    event EventHandler<MessageEventArgs> MessageReceived;
}