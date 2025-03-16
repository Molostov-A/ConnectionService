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
        // Ввод количества запросов
        Console.Write("Введите количество запросов: ");
        int requestCount = int.Parse(Console.ReadLine() ?? "10");

        // Ввод минимального и максимального userId
        Console.Write("Введите минимальное userId, которое будет использоваться: ");
        int userIdStart = int.Parse(Console.ReadLine() ?? "1000");

        Console.Write("Введите максимальное userId, которое будет использоваться: ");
        int userIdFinish = int.Parse(Console.ReadLine() ?? "10000");

        // Ввод задержки перед каждым запросом (в миллисекундах)
        Console.Write("Введите задержку перед каждым запросом (в миллисекундах): ");
        int delayMs = int.Parse(Console.ReadLine() ?? "0");

        // Ввод максимального числа параллельных запросов
        Console.Write("Введите максимальное количество параллельных запросов: ");
        int maxConcurrency = int.Parse(Console.ReadLine() ?? "100");

        Console.WriteLine("Запуск теста...");

        // Собираем результаты в список
        var results = new List<string>();

        var stopwatch = Stopwatch.StartNew();

        // Вызов теста с новыми параметрами
        await RunTestAsync(requestCount, userIdStart, userIdFinish, delayMs, maxConcurrency, results);

        stopwatch.Stop();

        // Теперь выводим результаты в консоль после завершения всех запросов
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }

        Console.WriteLine($"Все запросы отправлены! Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд.");

        Console.ReadLine();
    }

    static async Task RunTestAsync(int requestCount, int userIdStart, int userIdFinish, int delayMs, int maxConcurrency, List<string> results)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency); // Ограничение параллельных запросов
        var tasks = new List<Task>();

        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(SendRequestAsync(userIdStart, userIdFinish, delayMs, semaphore, results));
        }

        await Task.WhenAll(tasks);
    }

    static async Task SendRequestAsync(int userIdStart, int userIdFinish, int delayMs, SemaphoreSlim semaphore, List<string> results)
    {
        await semaphore.WaitAsync(); // Ограничиваем число параллельных запросов
        long userId = Random.Shared.Next(userIdStart, userIdFinish);
        try
        {
            await Task.Delay(delayMs); // Задержка перед отправкой            
            int typeProtocol = Random.Shared.Next(2) == 0 ? 4 : 6;
            string ip = typeProtocol == 4 ? IpGenerator.GenerateIPv4() : IpGenerator.GenerateIPv6();

            var request = new { Ip = ip };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{host}/api/users/{userId}/connect", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            // Собираем результаты в список (не выводим в консоль до завершения всех запросов)
            results.Add($"Ответ от {userId}: Статус {response.StatusCode}, Тело ответа: {responseBody}");
        }
        catch (Exception ex)
        {
            results.Add($"Ошибка при запросе для {userId}: {ex.Message}");
        }
        finally
        {
            semaphore.Release(); // Освобождаем слот
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
