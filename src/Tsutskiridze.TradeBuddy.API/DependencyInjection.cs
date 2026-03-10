using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using SharedKernel.Validations.Mediator;
using Tsutskiridze.TradeBuddy.Application;

namespace Tsutskiridze.TradeBuddy.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors = [typeof(ValidatorBehavior<,>)];
            })
            .AddMediatorValidators(typeof(AssemblyReference).Assembly);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                var jsonOptions = options.JsonSerializerOptions;

                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.WriteIndented = false;
                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddCors(options =>
        {
            string[] origins = [.. configuration["Cors:AllowedOrigins"]!.Split(',')];
            options.AddPolicy("AllowSpecificOrigins", (CorsPolicyBuilder builder) =>
            {
                builder.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
