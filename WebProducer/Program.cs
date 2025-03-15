using WebProducer;
using WebProducer.Configurations;
using WebProducer.Interfaces;
using WebProducer.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddScoped<IRequestProduser, RequestProduser>();
builder.Services.AddSingleton<ResponsePool>();
builder.Services.AddHostedService<ResponseConsumer>();

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
