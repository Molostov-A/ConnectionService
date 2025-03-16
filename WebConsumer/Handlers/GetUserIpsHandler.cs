using CommonData.Services;
using MessageBrokerModelsLibrary.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers;

public class GetUserIpsHandler : MessageHandler<GetUserIpsMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly JsonSerializerOptions _options;

    public GetUserIpsHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        var _options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };
    }

    public override async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender)
    {
        try
        {
            var connectionRequest = JsonSerializer.Deserialize<GetUserIpsMessage>(message);
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

                var result = await dataService.GetUserIpsAsync(connectionRequest.UserId);

                var response = new ResponseResult()
                {
                    Message = "Success",
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

