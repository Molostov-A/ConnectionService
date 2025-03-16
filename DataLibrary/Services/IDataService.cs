using DataLibrary.Models;

namespace CommonData.Services;

public interface IDataService
{
    Task<Connection> SaveConnectionAsync(long userId, string address, string protocol);

    Task<List<long>> GetUsersByIpAsync(string ipPart, string protocol);

    Task<List<string>> GetUserIpsAsync(long userId);

    Task<Connection> GetLatestConnectionAsync(long userId, string orderBy, string direction);

}
