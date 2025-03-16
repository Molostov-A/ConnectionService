using CommonData.Services;
using ModelsLibrary.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers;

public class ConnectUserHandler : MessageHandler<ConnectUserMessage>
{

    private readonly IDataService _dataService;

    public ConnectUserHandler(IDataService dataService)
    {
        _dataService = dataService;
    }

    public override async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender)
    {
        try
        {
            var connectionRequest = JsonSerializer.Deserialize<ConnectUserMessage>(message);
            if (connectionRequest == null)
            {
                var errorResponseResult = new ResponseResult()
                {
                    Message = "Error: failed to deserialize the message.",
                    Result = null,
                    Success = false
                };

                string errorResponse = JsonSerializer.Serialize(errorResponseResult);
                await messageSender.SendResponseAsync(correlationId, errorResponse);
                return;
            }

            var result = await _dataService.SaveConnectionAsync(connectionRequest.UserId, connectionRequest.IpAddress, connectionRequest.Protocol);

            var response = new ResponseResult()
            {
                Message = "Connection saved successfully.",
                Result = result,
                Success = true
            };

            string responseJson = JsonSerializer.Serialize(response);
            await messageSender.SendResponseAsync(correlationId, responseJson);
        }
        catch (Exception ex)
        {
            var response = new ResponseResult()
            {
                Message = ex.Message,
                Result = ex,
                Success = false
            };

            string errorResponse = JsonSerializer.Serialize(response);
            await messageSender.SendResponseAsync(correlationId, errorResponse);
        }
    }
}

