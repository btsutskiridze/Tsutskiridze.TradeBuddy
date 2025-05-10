using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tsutskiridze.Bloom.Core;
using Tsutskiridze.Bloom.Core.Common.Options;
using Tsutskiridze.Bloom.Core.Configurations;
using Tsutskiridze.TradeBuddy.Application.Jobs;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTradeBuddyServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddCoreServices(configuration)
                .AddAppOptions(configuration)
                .AddApplicationServices()
                .AddInfrastructureServices()
                .AddDatabaseServices(configuration);

            return services;
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddControllers();

            services.AddBloom((opt) =>
            {
                opt.AddMail = true;
                opt.AddJobs = true;

                opt.CorsOptions = new CorsConfOptions
                {
                    ConfigurePolicy = (policy) =>
                    {
                        string[] origins = [.. (configuration["Cors:AllowedOrigins"]!.Split(','))];
                        policy.AddPolicy("AllowSpecificOrigins", delegate (CorsPolicyBuilder b)
                        {
                            b.WithOrigins(origins)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                        });
                    }
                };

                opt.JsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
            });

            services.ConfigureBloomServices((s) =>
            {
                s.AddJob<StockBuddyJob>();
            });

            return services;
        }
    }
}