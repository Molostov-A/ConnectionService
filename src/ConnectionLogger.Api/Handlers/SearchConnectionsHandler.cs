using ConnectionLogger.Data.Services;
using ConnectionLogger.Messaging.Messages;
using ConnectionLogger.Api.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConnectionLogger.Api.Handlers;

public class SearchConnectionsHandler : MessageHandler<SearchConnectionsMessage>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly JsonSerializerOptions _options;

    public SearchConnectionsHandler(IServiceScopeFactory serviceScopeFactory)
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
            var connectionRequest = JsonSerializer.Deserialize<SearchConnectionsMessage>(message);
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

                var result = await dataService.GetLatestConnectionAsync(connectionRequest.UserId, connectionRequest.OrderBy, connectionRequest.Direction);

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
