using DataLibrary.Models;

namespace CommonData.Services;

public interface IDataService
{
    Task<Connection> SaveConnectionAsync(long userId, string address, string protocol);

}
