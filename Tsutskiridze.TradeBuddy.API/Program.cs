using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tsutskiridze.Bloom.Core;
using Tsutskiridze.Bloom.Core.Common.Options;
using Tsutskiridze.Bloom.Core.Configurations;
using Tsutskiridze.TradeBuddy.API.Extensions;
using Tsutskiridze.TradeBuddy.Application.Jobs;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTradeBuddyServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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
