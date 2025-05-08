using CurrencyConverterAPI.Intefaces;
using CurrencyConverterAPI.Policies;
using CurrencyConverterAPI.Services;
using Polly;
using Polly.Extensions.Http;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("Frankfurter", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app/");
})
.AddPolicyHandler(RetryPolicy.GetRetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy.GetCircuitBreakerPolicy());

builder.Services.AddScoped<ICurrencyConversionService, ExchangeService>();
builder.Services.AddScoped<ICurrencyRateService, CurrencyRateService>();

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
