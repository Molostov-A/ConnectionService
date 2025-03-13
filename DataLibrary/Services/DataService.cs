using DataLibrary;
using DataLibrary.Models;

namespace CommonData.Services;

public class DataService : IDataService
{
    private readonly AppDbContext _dbContext;

    public DataService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveConnectionAsync(long userId, string address, string protocol)
    {
        // Добавляем IP-адрес
        var ipAddress = new IpAddress { Address = address, Protocol = protocol };
        await _dbContext.IpAddresses.AddAsync(ipAddress);

        // Добавляем пользователя
        var user = new User { Id = userId, FirstName = "John", LastName = "Doe" }; // Пример
        await _dbContext.Users.AddAsync(user);

        // Добавляем соединение
        var connection = new Connection { UserId = user.Id, IpAddressId = ipAddress.Id };
        await _dbContext.Connections.AddAsync(connection);

        // Сохраняем все изменения в БД
        await _dbContext.SaveChangesAsync();
    }
}
