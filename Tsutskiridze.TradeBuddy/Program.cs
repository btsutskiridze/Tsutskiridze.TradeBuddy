using Microsoft.AspNetCore.Cors.Infrastructure;
using Tsutskiridze.Bloom.Core;
using Tsutskiridze.Bloom.Core.Common.Options;
using Tsutskiridze.Bloom.Core.Configurations;
using Tsutskiridze.Bloom.Core.Secrets;
using Tsutskiridze.TradeBuddy.Jobs;
using Tsutskiridze.TradeBuddy.Services.Stocks;

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
});

builder.Services.AddTransient<AlphaVantageService>();
builder.Services.ConfigureBloomServices((s) =>
{
    s.AddJob<StockBuddyJob>();
});


var app = builder.Build();

app.UseBloom();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
