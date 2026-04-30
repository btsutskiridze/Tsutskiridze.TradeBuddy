using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("chats");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.ActivationToken)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(c => c.PrivateName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(c => c.TelegramChatId)
                .IsRequired(false);
            
            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
            
            builder.HasIndex(c => c.ActivationToken)
                .IsUnique()
                .HasDatabaseName("UX_chats_activation_token");
            
            builder.HasIndex(c => c.TelegramChatId)
                .IsUnique()
                .HasDatabaseName("UX_chats_telegram_chat_id");
        }
    }
}

