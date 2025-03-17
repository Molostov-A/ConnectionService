using ConnectionLogger.Data.Models;
using ConnectionLogger.Messaging.Messages;

namespace ConnectionLogger.Data.Services;

public interface IDataService
{
    Task<Connection> SaveConnectionAsync(long userId, string address, string protocol);

    Task<List<long>> GetUsersByIpAsync(string ipPart, string protocol);

    Task<List<string>> GetUserIpsAsync(long userId);

    Task<Connection> GetLatestConnectionAsync(long userId, OrderBy orderBy, Direction direction);
}
