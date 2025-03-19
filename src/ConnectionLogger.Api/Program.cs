using ConnectionLogger.Data;
using ConnectionLogger.Data.Services;
using ConnectionLogger.Api.Configurations;
using ConnectionLogger.Api.Handlers;
using ConnectionLogger.Api.Interfaces;
using ConnectionLogger.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IDataService, DataService>();

// Регистрация обработчиков сообщений из RabbitMQ
builder.Services.AddSingleton<IMessageHandler, ConnectUserHandler>();

builder.Services.AddHostedService<RequestConsumerService>();

builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();