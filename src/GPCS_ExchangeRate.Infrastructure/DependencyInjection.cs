using GPCS_ExchangeRate.Application.Interfaces.Configurations;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Common.Interfaces;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Configurations;
using GPCS_ExchangeRate.Infrastructure.Data;
using GPCS_ExchangeRate.Infrastructure.Repositories;
using GPCS_ExchangeRate.Infrastructure.Services;
using GPCS_ExchangeRate.Infrastructure.Services.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GPCS_ExchangeRate.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Configure Settings ────────────────────────────────────────────────
            services.Configure<DocumentApiSettings>(
                configuration.GetSection("ExternalApis:DocumentApi"));

            services.Configure<WorkflowConfiguration>(
                configuration.GetSection("WorkflowConfiguration"));

            services.Configure<MockUserOptions>(
                configuration.GetSection(MockUserOptions.SectionName));

            var mockOptions = configuration
                .GetSection(MockUserOptions.SectionName)
                .Get<MockUserOptions>();

            if(mockOptions?.Enabled == true)
            {
                services.AddScoped<IUserAccountService, MockUserAccountService>();
            }

            // ── Database ──────────────────────────────────────────────────────────
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName)));

            // ── Repositories & Unit of Work ───────────────────────────────────────
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IExchangeRateHeaderRepository, ExchangeRateHeaderRepository>();
            services.AddScoped<IExchangeRateDetailRepository, ExchangeRateDetailRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── Services ──────────────────────────────────────────────────────────
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IDateTimeService, DateTimeService>();

            // ── External Services ─────────────────────────────────────────────────
            services.AddHttpClient<IDocumentService, DocumentService>((sp,client) =>
            {
                var settings = sp.GetRequiredService<IOptions<DocumentApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.Timeout);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    UseDefaultCredentials = true
                };
            });

            services.AddSingleton<IWorkflowConfiguration>(sp =>
                sp.GetRequiredService<IOptions<WorkflowConfiguration>>().Value);

            services.AddScoped<IExternalDbService, ExternalDbService>();

            return services;
        }
    }
}
