using CommonData.Services;
using MessageBrokerModelsLibrary.Models;
using System.Text.Json;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers;

public class UserConnectionHandler : MessageHandler<UserConnectionMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UserConnectionHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender)
    {
        try
        {
            var connectionRequest = JsonSerializer.Deserialize<UserConnectionMessage>(message);
            if (connectionRequest == null)
            {
                await messageSender.SendResponseAsync(correlationId, "Ошибка: не удалось десериализовать сообщение.");
                return;
            }

            long userId = connectionRequest.UserId;
            string address = connectionRequest.IpAddress;
            string protocol = connectionRequest.Protocol;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

                var result = await dataService.SaveConnectionAsync(userId, address, protocol);

                var response = new
                {
                    Message = "Соединение успешно сохранено",
                    UserId = result.UserId,
                    IpAddress = result.IpAddress.Address,
                    Protocol = result.IpAddress.Protocol,
                    ConnectedAt = result.ConnectedAt
                };

                string responseJson = JsonSerializer.Serialize(response);
                await messageSender.SendResponseAsync(correlationId, responseJson);
            }
        }
        catch (Exception ex)
        {
            string errorResponse = JsonSerializer.Serialize(new { Error = ex.Message });
            await messageSender.SendResponseAsync(correlationId, errorResponse);
        }
    }
}

