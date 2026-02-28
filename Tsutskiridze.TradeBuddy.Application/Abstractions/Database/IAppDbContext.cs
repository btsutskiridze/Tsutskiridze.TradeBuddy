using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.Database
{
    public interface IAppDbContext
    {
        /// <summary>
        /// Get the DbSet for any entity T – add/update/remove/find via this.
        /// </summary>
        DbSet<T> Set<T>() where T : class;

        /// <summary>
        /// Flush tracked changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Access EF’s lower-level features: transactions, raw SQL, etc.
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// Inspect change tracker / detach entities if you need to.
        /// </summary>
        ChangeTracker ChangeTracker { get; }
    }
}
