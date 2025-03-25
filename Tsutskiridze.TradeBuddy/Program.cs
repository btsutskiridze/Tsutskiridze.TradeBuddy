using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tsutskiridze.Bloom.Core;
using Tsutskiridze.Bloom.Core.Common.Options;
using Tsutskiridze.Bloom.Core.Configurations;
using Tsutskiridze.TradeBuddy.Configs;
using Tsutskiridze.TradeBuddy.Jobs;
using Tsutskiridze.TradeBuddy.Mappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBloom((opt) =>
{
    opt.AddMail = true;
    opt.AddJobs = true;

    opt.CorsOptions = new CorsConfOptions
    {
        ConfigurePolicy = (policy) =>
        {
            string[] origins = [.. (SecretsManager.GetSecret("Cors:AllowedOrigins").Split(','))];
            policy.AddPolicy("AllowSpecificOrigins", delegate (CorsPolicyBuilder b)
            {
                b.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()
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

builder.Services.AddControllers();

builder.Services.AddHttpClientConfigs();

builder.Services.AddServiceConfigs();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.ConfigureBloomServices((s) =>
{
    s.AddJob<StockBuddyJob>();
});


var app = builder.Build();

app.UseBloom();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
