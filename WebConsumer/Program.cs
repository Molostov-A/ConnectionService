
using CommonData.Services;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using WebConsumer.Configurations;
using WebConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // Вынесем строку подключения в appsettings.json

builder.Services.AddScoped<IDataService, DataService>();

builder.Services.AddScoped<ConsumerService>();
builder.Services.AddHostedService(provider =>
{
    return provider.GetRequiredService<ConsumerService>();  // Используем scoped сервис в hosted
}); 


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Миграции и создание базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Это выполнит миграцию и создаст таблицы
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
