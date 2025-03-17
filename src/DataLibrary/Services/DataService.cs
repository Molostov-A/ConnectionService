using ConnectionLogger.Data.Models;
using ConnectionLogger.Messaging.Messages;
using Microsoft.EntityFrameworkCore;

namespace ConnectionLogger.Data.Services;

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
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IpAddressId == ipAddress.Id &&
                                          c.ConnectedAt.Date == DateTime.UtcNow.Date);

            if (existingConnection != null)
            {
                return existingConnection;
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
            .OrderBy(c => c.IpAddress.Protocol)
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

        query = query.Where(c => c.UserId == userId);

        if (orderBy == OrderBy.DateCreated)
        {
            query = direction == Direction.Asc
                ? query.OrderBy(c => c.ConnectedAt)
                : query.OrderByDescending(c => c.ConnectedAt);
        }
        else if (orderBy == OrderBy.IpAddress)
        {
            query = direction == Direction.Asc
                ? query.OrderBy(c => c.IpAddressId)
                : query.OrderByDescending(c => c.IpAddressId);
        }
        else if (orderBy == OrderBy.UserId)
        {
            query = direction == Direction.Asc
                ? query.OrderBy(c => c.UserId)
                : query.OrderByDescending(c => c.UserId);
        }
        else
        {
            throw new ArgumentException("Invalid orderBy value");
        }

        return await query.FirstOrDefaultAsync();
    }
}
