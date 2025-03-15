
using CommonData.Services;
using DataLibrary;
using MessageBrokerModelsLibrary.Configurations;
using MessageBrokerToolkit.Interfaces;
using MessageBrokerToolkit.Services;
using Microsoft.EntityFrameworkCore;
using WebConsumer.Configurations;
using WebConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация настроек
builder.Services.Configure<AppSettings>(builder.Configuration);
//builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Регистрация ConsumerServiceMBT для AppSettings
builder.Services.AddScoped(typeof(ConsumerServiceMBT<AppSettings>));
builder.Services.AddScoped<IProduserServiceMBT, ProduserServiceMBT<AppSettings>>();

// Получаем строку подключения из конфигурации
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрация DbContext с использованием строки подключения
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IConsumerService, ConsumerService>();
builder.Services.AddScoped<ConsumerBackgroundService>();

// Регистрация Hosted Service с использованием фабрики
builder.Services.AddHostedService<ConsumerServiceHosted>();

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