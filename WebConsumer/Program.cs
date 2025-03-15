
using CommonData.Services;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using WebConsumer;
using WebConsumer.Configurations;
using WebConsumer.Handlers;
using WebConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация настроек
builder.Services.Configure<AppSettings>(builder.Configuration);
// Получаем строку подключения из конфигурации
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрация DbContext с использованием строки подключения
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Регистрация сервиса для работы с базой данных
builder.Services.AddScoped<IDataService, DataService>();

// Регистрация сервиса для отправки сообщений
builder.Services.AddSingleton<IMessageSender, RabbitMQMessageSender>();

// Регистрация обработчиков
builder.Services.AddSingleton<IMessageHandler, TypeAHandler>();
builder.Services.AddSingleton<IMessageHandler, TypeBHandler>();
builder.Services.AddSingleton<IMessageHandler, UserConnectionHandler>();

// Регистрация ConsumerBackgroundService
builder.Services.AddHostedService<ConsumerBackgroundService>();

// Добавление поддержки контроллеров
builder.Services.AddControllers();
// Добавление поддержки Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Создание приложения
var app = builder.Build();

// Миграции и создание базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Выполнение миграций
}

// Конфигурация HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Запуск приложения
app.Run();