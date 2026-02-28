using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("chats");

            builder.HasKey(c => c.ID);

            builder.HasMany(c => c.PriceAlerts)
                .WithOne(pa => pa.Chat)
                .HasForeignKey(pa => pa.ChatID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
