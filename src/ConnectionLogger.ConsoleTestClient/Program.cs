using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static string host = "https://localhost:";

    static async Task Main()
    {

        Console.Write("Порт (https://localhost:<port>) :");
        int port = int.Parse(Console.ReadLine() ?? "7007");
        host += port;
        Console.Write("Количество запросов: ");
        int requestCount = int.Parse(Console.ReadLine() ?? "10");

        Console.Write("Минимальное userId, которое будет использоваться: ");
        int userIdStart = int.Parse(Console.ReadLine() ?? "1000");

        Console.Write("Максимальное userId, которое будет использоваться: ");
        int userIdFinish = int.Parse(Console.ReadLine() ?? "10000");

        Console.Write("Задержка перед каждым запросом (в миллисекундах): ");
        int delayMs = int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Максимальное количество параллельных запросов: ");
        int maxConcurrency = int.Parse(Console.ReadLine() ?? "100");

        Console.WriteLine("Запуск теста...");

        var results = new List<string>();

        var stopwatch = Stopwatch.StartNew();

        await RunTestAsync(requestCount, userIdStart, userIdFinish, delayMs, maxConcurrency, results);

        stopwatch.Stop();

        foreach (var result in results)
        {
            Console.WriteLine(result);
        }

        Console.WriteLine($"Все запросы отправлены! Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд.");

        Console.ReadLine();
    }

    static async Task RunTestAsync(int requestCount, int userIdStart, int userIdFinish, int delayMs, int maxConcurrency, List<string> results)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = new List<Task>();

        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(SendRequestAsync(userIdStart, userIdFinish, delayMs, semaphore, results));
        }

        await Task.WhenAll(tasks);
    }

    static async Task SendRequestAsync(int userIdStart, int userIdFinish, int delayMs, SemaphoreSlim semaphore, List<string> results)
    {
        await semaphore.WaitAsync();
        long userId = Random.Shared.Next(userIdStart, userIdFinish);
        try
        {
            await Task.Delay(delayMs);
            int typeProtocol = Random.Shared.Next(2) == 0 ? 4 : 6;
            string ip = typeProtocol == 4 ? IpGenerator.GenerateIPv4() : IpGenerator.GenerateIPv6();

            var request = new { Ip = ip };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{host}/api/users/{userId}/connect", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            results.Add($"Ответ от {userId}: Статус {response.StatusCode}, Тело ответа: {responseBody}");
        }
        catch (Exception ex)
        {
            results.Add($"Ошибка при запросе для {userId}: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
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
