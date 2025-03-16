using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Random random = new Random();
    private static readonly string host = "https://localhost:7007";

    static async Task Main()
    {
        var tasks = new ConcurrentBag<Task>();

        await Parallel.ForEachAsync(Enumerable.Range(0, 5), async (_, _) =>
        {
            long UserId = random.Next(1000, 100500);
            int typeProtocol = random.Next(2) == 0 ? 4 : 6;
            string ip = typeProtocol == 4 ? GeneratorIp.GenerateIPv4() : GeneratorIp.GenerateIPv6();

            var request = new { Ip = ip };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync($"{host}/api/users/{UserId}/connect", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке запроса: {ex.Message}");
            }
        });

        Console.WriteLine("All requests sent!");
    }
}

public static class GeneratorIp
{
    private static readonly Random random = new Random();
    private const string hexChars = "0123456789abcdef";

    public static string GenerateIPv4()
    {
        return $"{random.Next(0, 256)}.{random.Next(0, 256)}.{random.Next(0, 256)}.{random.Next(0, 256)}";
    }

    public static string GenerateIPv6()
    {
        var ipv6 = new StringBuilder();
        for (int i = 0; i < 8; i++)
        {
            ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            if (i < 7) ipv6.Append(':');
        }
        return ipv6.ToString();
    }
}
