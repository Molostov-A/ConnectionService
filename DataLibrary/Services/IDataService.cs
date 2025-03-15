using DataLibrary.Models;

namespace CommonData.Services;

public interface IDataService
{
    Task<object> SaveConnectionAsync(long userId, string address, string protocol);

}
