using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTradeBuddyServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddCoreServices(configuration)
                .AddAppOptions(configuration)
                .AddApplicationServices()
                .AddInfrastructureServices()
                .AddDatabaseServices(configuration);

            return services;
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddControllers()
                .AddJsonOptions(opt => {
                    var jOptions = opt.JsonSerializerOptions;

                    jOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jOptions.PropertyNameCaseInsensitive = true;
                    jOptions.WriteIndented = false;
                    jOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddCors((options) =>
            {
                string[] origins = [.. (configuration["Cors:AllowedOrigins"]!.Split(','))];
                options.AddPolicy("AllowSpecificOrigins", delegate(CorsPolicyBuilder b)
                {
                    b.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}