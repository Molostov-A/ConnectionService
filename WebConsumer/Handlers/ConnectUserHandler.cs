using CommonData.Services;
using MessageBrokerModelsLibrary.Models;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers;

public class ConnectUserHandler : MessageHandler<ConnectUserMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly JsonSerializerOptions _options;
    public ConnectUserHandler(IServiceScopeFactory serviceScopeFactory)
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
            var connectionRequest = JsonSerializer.Deserialize<ConnectUserMessage>(message);
            if (connectionRequest == null)
            {
                var response = new ResponseResult()
                {
                    Message = "Error: failed to deserialize the message.",
                    Result = null,
                    Success = false
                };
                string errorResponse = JsonSerializer.Serialize(response, _options);
                await messageSender.SendResponseAsync(correlationId, errorResponse);
                return;
            }



            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

                var result = await dataService.SaveConnectionAsync(connectionRequest.UserId, connectionRequest.IpAddress, connectionRequest.Protocol);

                var response = new ResponseResult()
                {
                    Message = "Connection saved successfully",
                    Result = result,
                    Success = true
                };


                string responseJson = JsonSerializer.Serialize(response, _options);
                await messageSender.SendResponseAsync(correlationId, responseJson);
            }
        }
        catch (Exception ex)
        {
            var response = new ResponseResult()
            {
                Message = ex.Message,
                Result = ex,
                Success = false
            };
            string errorResponse = JsonSerializer.Serialize(response, _options);
            await messageSender.SendResponseAsync(correlationId, errorResponse);
        }
    }
}

