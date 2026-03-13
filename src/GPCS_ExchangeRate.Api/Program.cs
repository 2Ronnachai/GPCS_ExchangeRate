using GPCS_ExchangeRate.Api.Handlers;
using GPCS_ExchangeRate.Application;
using GPCS_ExchangeRate.Infrastructure;
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

var app = builder.Build();

// --- Middleware ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
