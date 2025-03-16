using DataLibrary;
using DataLibrary.Models;
using MessageBrokerModelsLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonData.Services;

public class DataService : IDataService
{
    private readonly AppDbContext _dbContext;

    public DataService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Connection> SaveConnectionAsync(long userId, string address, string protocol)
    {
        try
        {
            var ipAddress = await _dbContext.IpAddresses
                .FirstOrDefaultAsync(ip => ip.Address == address && ip.Protocol == protocol);

            if (ipAddress == null)
            {
                ipAddress = new IpAddress { Address = address, Protocol = protocol };
                await _dbContext.IpAddresses.AddAsync(ipAddress);
                await _dbContext.SaveChangesAsync();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                user = new User { Id = userId, FirstName = "unknown", LastName = "unknown" };
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }

            var existingConnection = await _dbContext.Connections
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IpAddressId == ipAddress.Id);

            if (existingConnection != null)
            {
                return existingConnection; // Возвращаем, если соединение уже существует
            }

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
        catch (Exception ex)
        {
            throw new Exception("Error when saving a connection", ex);
        }
    }

    public async Task<List<long>> GetUsersByIpAsync(string ipPart, string protocol)
    {
        return await _dbContext.Connections
            .OrderBy(c => c.IpAddress.Protocol) // Сортировка по протоколу
            .Where(c => c.IpAddress.Address.StartsWith(ipPart) && c.IpAddress.Protocol == protocol)
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<string>> GetUserIpsAsync(long userId)
    {
        return await _dbContext.Connections
            .Where(c => c.UserId == userId)
            .Select(c => c.IpAddress.Address)
            .ToListAsync();
    }

    public async Task<Connection> GetLatestConnectionAsync(long userId, OrderBy orderBy, Direction direction)
    {
        var query = _dbContext.Connections.AsQueryable();

        // Применяем фильтрацию по userId
        query = query.Where(c => c.UserId == userId);

        // Применяем сортировку в зависимости от параметров
        if (orderBy == OrderBy.dateCreated)
        {
            query = direction == Direction.asc
                ? query.OrderBy(c => c.ConnectedAt)
                : query.OrderByDescending(c => c.ConnectedAt);
        }
        else if (orderBy == OrderBy.ipAddress)
        {
            query = direction == Direction.asc
                ? query.OrderBy(c => c.IpAddressId)
                : query.OrderByDescending(c => c.IpAddressId);
        }
        else if (orderBy == OrderBy.userId)
        {
            query = direction == Direction.asc
                ? query.OrderBy(c => c.UserId)
                : query.OrderByDescending(c => c.UserId);
        }
        else
        {
            throw new ArgumentException("Invalid orderBy value");
        }

        // Получаем только одну запись, которая будет крайним подключением
        return await query.FirstOrDefaultAsync();
    }
}
