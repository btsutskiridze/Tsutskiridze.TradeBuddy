using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

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
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

