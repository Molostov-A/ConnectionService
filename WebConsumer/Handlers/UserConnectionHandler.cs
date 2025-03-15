using CommonData.Services;
using MessageBrokerModelsLibrary.Models;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers;

public class UserConnectionHandler : MessageHandler<UserConnectionMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly JsonSerializerOptions _options;
    public UserConnectionHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        var _options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Отключает Unicode-экранирование
            WriteIndented = false
        };
    }

    public override async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender)
    {
        try
        {
            var connectionRequest = JsonSerializer.Deserialize<UserConnectionMessage>(message);
            if (connectionRequest == null)
            {
                await messageSender.SendResponseAsync(correlationId, "Error: failed to deserialize the message.");
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
                    Message = "Connection saved successfully",
                    UserId = result.UserId,
                    IpAddress = result.IpAddress.Address,
                    Protocol = result.IpAddress.Protocol,
                    ConnectedAt = result.ConnectedAt
                };

                string responseJson = JsonSerializer.Serialize(response, _options);
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

