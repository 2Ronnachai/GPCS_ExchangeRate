using GPCS_ExchangeRate.Api.Middleware;
using GPCS_ExchangeRate.Api.Services;
using GPCS_ExchangeRate.Application.Common.Mappings;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using GPCS_ExchangeRate.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Current User ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- Unit of Work ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- MediatR ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands
            .CreateExchangeRate.CreateExchangeRateCommand).Assembly));

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(ExchangeRateProfile).Assembly);

// --- Controllers & Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GPCS ExchangeRate API", Version = "v1" });
});

var app = builder.Build();

// --- Middleware ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
