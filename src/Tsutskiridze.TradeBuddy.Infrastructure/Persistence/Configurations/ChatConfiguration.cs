using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            /*
             *todo:
             *Add indexes and uniqueness rules for ActivationToken and TelegramChatId,
             *plus required column constraints if the lifecycle requires them.
             * 
             */
            
            builder.ToTable("chats");

            builder.HasKey(c => c.Id);
        }
    }
}

