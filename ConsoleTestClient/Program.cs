using System;
using System.Diagnostics;
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
        Console.Write("Введите количество запросов: ");
        int requestCount = int.Parse(Console.ReadLine() ?? "10");

        Console.Write("Введите количество потоков: ");
        int threadCount = int.Parse(Console.ReadLine() ?? "5");

        Console.WriteLine("Запуск теста...");
        var stopwatch = Stopwatch.StartNew();

        await RunTestAsync(requestCount, threadCount);

        stopwatch.Stop();
        Console.WriteLine($"Все запросы отправлены! Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд.");
    }

    static async Task RunTestAsync(int requestCount, int threadCount)
    {
        await Parallel.ForEachAsync(Enumerable.Range(0, requestCount), new ParallelOptions { MaxDegreeOfParallelism = threadCount }, async (_, _) =>
        {
            long userId = random.Next(1000, 100500);
            int typeProtocol = random.Next(2) == 0 ? 4 : 6;
            string ip = typeProtocol == 4 ? GeneratorIp.GenerateIPv4() : GeneratorIp.GenerateIPv6();

            var request = new { Ip = ip };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync($"{host}/api/users/{userId}/connect", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке запроса: {ex.Message}");
            }
        });
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
            for (int j = 0; j < 4; j++)
            {
                ipv6.Append(hexChars[random.Next(hexChars.Length)]);
            }
            if (i < 7) ipv6.Append(':');
        }
        return ipv6.ToString();
    }
}
