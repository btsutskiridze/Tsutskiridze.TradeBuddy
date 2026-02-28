using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<PriceAlert> PriceAlerts { get; set; }

        // IAppDbContext
        DbSet<T> IAppDbContext.Set<T>() => base.Set<T>();
        Task<int> IAppDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
        DatabaseFacade IAppDbContext.Database => base.Database;
        ChangeTracker IAppDbContext.ChangeTracker => base.ChangeTracker;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
