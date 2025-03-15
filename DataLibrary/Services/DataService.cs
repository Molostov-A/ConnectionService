using DataLibrary;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonData.Services;

public class DataService : IDataService
{
    private readonly AppDbContext _dbContext;

    public DataService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object> SaveConnectionAsync(long userId, string address, string protocol)
    {
        // Проверяем, существует ли IP-адрес
        var ipAddress = await _dbContext.IpAddresses
            .FirstOrDefaultAsync(ip => ip.Address == address && ip.Protocol == protocol);

        if (ipAddress == null)
        {
            ipAddress = new IpAddress { Address = address, Protocol = protocol };
            await _dbContext.IpAddresses.AddAsync(ipAddress);
            await _dbContext.SaveChangesAsync(); // 💾 Сохраняем, чтобы получить Id
        }

        // Проверяем, существует ли пользователь
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            user = new User { Id = userId, FirstName = "unknown", LastName = "unknown" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync(); // 💾 Сохраняем, чтобы получить Id
        }

        // Проверяем существующее соединение
        var existingConnection = await _dbContext.Connections
            .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IpAddressId == ipAddress.Id);

        if (existingConnection != null)
        {
            return existingConnection; // Возвращаем уже существующее соединение
        }

        // Создаём новое соединение
        var connection = new Connection
        {
            User = user,
            IpAddress = ipAddress,
            ConnectedAt = DateTime.UtcNow
        };

        await _dbContext.Connections.AddAsync(connection);
        await _dbContext.SaveChangesAsync();

        return connection;
    }
}
