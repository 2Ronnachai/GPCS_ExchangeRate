using GPCS_ExchangeRate.Api.Handlers;
using GPCS_ExchangeRate.Application;
using GPCS_ExchangeRate.Infrastructure;
using GPCS_ExchangeRate.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// --- Application (MediatR, AutoMapper) ---
builder.Services.AddApplication();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

// --- Infrastructure (EF Core, Repositories, UnitOfWork, Services) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Exception Handling ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Controllers & Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GPCS ExchangeRate API", Version = "v1" });
});

// --- Background Services ---
builder.Services.AddHostedService<OutboxProcessorService>();

// Cor Configuration
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? [];
var allowAnyOrigin = corsSettings.GetValue<bool>("AllowAnyOrigin");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowAnyOrigin)
        {
            // Development: Allow any origin
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else if (allowedOrigins.Length > 0)
        {
            // Production: Specify allowed origins
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Fallback: No CORS configuration found
            throw new InvalidOperationException(
                "CORS configuration is required!"
            );
        }
    });
});

var app = builder.Build();

// --- Middleware ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
