using Microsoft.EntityFrameworkCore;
using SharedKernel.Outbox;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Stocks;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<PriceAlert> PriceAlerts { get; set; }
        public DbSet<TradeStrategy> TradeStrategies { get; set; }
        public DbSet<StrategyMonitor> StrategyMonitors { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

