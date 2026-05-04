using Tsutskiridze.TradeBuddy.API;
using Tsutskiridze.TradeBuddy.API.Telegram;
using Tsutskiridze.TradeBuddy.Application;
using Tsutskiridze.TradeBuddy.Domain;
using Tsutskiridze.TradeBuddy.Infrastructure;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddTelegramPresentation()
    .AddDomainServices()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ApplyDatabaseMigrationsAsync();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseScalarUi();
}

app.Run();
