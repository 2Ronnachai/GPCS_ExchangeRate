using FluentValidation;
using GPCS_ExchangeRate.Application.Common.Behaviours;
using GPCS_ExchangeRate.Application.Common.Mappings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GPCS_ExchangeRate.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Auto-register validators from the assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                // Add the validation behavior to the MediatR pipeline
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // AutoMapper
            services.AddAutoMapper(typeof(ExchangeRateProfile).Assembly);

            return services;
        }
    }
}
