using WebProducer.Configurations;
using IConsumerServiceMBT = MessageBrokerToolkit.Interfaces.IConsumerServiceMBT;
using IProduserServiceMBT = MessageBrokerToolkit.Interfaces.IProduserServiceMBT;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddScoped<IProduserServiceMBT, MessageBrokerToolkit.Services.ProduserServiceMBT<AppSettings>>();
builder.Services.AddScoped<IConsumerServiceMBT, MessageBrokerToolkit.Services.ConsumerServiceMBT<AppSettings>>();

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
