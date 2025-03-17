using ConnectionLogger.AsyncReceiver;
using ConnectionLogger.AsyncReceiver.Configurations;
using ConnectionLogger.AsyncReceiver.Interfaces;
using ConnectionLogger.AsyncReceiver.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddScoped<IRequestProduser, RequestProduserService>();
builder.Services.AddSingleton<ResponsePool>();
builder.Services.AddHostedService<ResponseConsumerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
