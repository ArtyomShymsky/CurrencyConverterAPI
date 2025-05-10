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
using System.Text;
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
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddScoped<ICurrencyConversionService, ExchangeService>();
builder.Services.AddScoped<ICurrencyRateService, CurrencyRateService>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    //options.ApiVersionReader = new HeaderApiVersionReader("api-version");

    //options.ApiVersionReader = ApiVersionReader.Combine(
    //    new UrlSegmentApiVersionReader(),
    //    new HeaderApiVersionReader("X-Api-Version")
    //);
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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer <your JWT token>"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

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


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    //options.Events = new JwtBearerEvents
    //{
    //    OnChallenge = context =>
    //    {
    //        // Avoid default behavior
    //        context.HandleResponse();

    //        context.Response.StatusCode = 401;
    //        context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"yourapp\", error=\"invalid_token\", error_description=\"{context.ErrorDescription}\"");
    //        return context.Response.WriteAsync("Unauthorized: Token is invalid or missing");
    //    }
    //};


});



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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
