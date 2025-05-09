using Asp.Versioning;
using CurrencyConverterAPI.Intefaces;
using CurrencyConverterAPI.Middleware;
using CurrencyConverterAPI.Policies;
using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");

    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    )
    ;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    //options.SwaggerDoc("v2", new OpenApiInfo { Title = "Currency API", Version = "v2" });
});

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)))
        .AddConsoleExporter();
});

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddConsoleExporter())
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddConsoleExporter());




builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Set issuer and audience here
        };
    });

//builder.Services.ConfigureOpenTelemetryTracerProvider(builder =>
//{
//    builder
//        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyAspNetService"))
//        .AddAspNetCoreInstrumentation(); // Automatic instrumentation for ASP.NET Core
//       // .AddHttpClientInstrumentation() // Automatically instrument HTTP client requests
//        //.AddJaegerExporter(options =>
//        //{
//        //    options.AgentHost = "localhost";  // Jaeger agent host
//        //    options.AgentPort = 6831; // Default Jaeger port
//        //});
//});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Optionally log to console
    .WriteTo.Seq("http://localhost:5341") // URL of your Seq server
    .CreateLogger();
builder.Host.UseSerilog();




var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency API v1");
        //options.SwaggerEndpoint("/swagger/v2/swagger.json", "Currency API v2");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
