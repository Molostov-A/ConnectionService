using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Random random = new Random();
    private static readonly string host = "https://localhost:7007";

    static async Task Main()
    {
        Console.Write("Введите количество запросов: ");
        int requestCount = int.Parse(Console.ReadLine() ?? "10");

        Console.Write("Введите минимальное userId, которое будет использоваться: ");
        int userIdStart = int.Parse(Console.ReadLine() ?? "1000");

        Console.Write("Введите максимальное userId, которое будет использоваться: ");
        int userIdFinish = int.Parse(Console.ReadLine() ?? "10000");

        Console.WriteLine("Запуск теста...");
        var stopwatch = Stopwatch.StartNew();

        await RunTestAsync(requestCount, userIdStart, userIdFinish);

        stopwatch.Stop();
        Console.WriteLine($"Все запросы отправлены! Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд.");
        Console.ReadLine();
    }

    static async Task RunTestAsync(int requestCount, int userIdStart, int userIdFinish)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(SendRequestAsync(userIdStart, userIdFinish));
        }

        await Task.WhenAll(tasks);
    }

    static async Task SendRequestAsync(int userIdStart, int userIdFinish)
    {
        long userId = random.Next(userIdStart, userIdFinish);
        int typeProtocol = random.Next(2) == 0 ? 4 : 6;
        string ip = typeProtocol == 4 ? IpGenerator.GenerateIPv4() : IpGenerator.GenerateIPv6();

        var request = new { Ip = ip };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{host}/api/users/{userId}/connect", content);
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ответ [{response.StatusCode}]: {responseBody}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке запроса: {ex.Message}");
        }
    }
}

public static class IpGenerator
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
            for (int j = 0; j < 4; j++)
            {
                ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            }

            if (i < 7)
            {
                ipv6.Append(':');
            }
        }

        return ipv6.ToString();
    }
}
