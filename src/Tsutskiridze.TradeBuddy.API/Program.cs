using Microsoft.EntityFrameworkCore;
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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

if (app.Environment.IsDevelopment())
{
    app.UseScalarUi();
}

app.Run();
