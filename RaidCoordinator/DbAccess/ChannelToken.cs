using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace RaidCoordinator
{
    public class ChannelToken
    {
        public byte[] ChannelId { get; set; }
        public byte[] GuildId { get; set; }
        public int Token { get; set; }
    }

    public class ChannelTokenConfig : IEntityTypeConfiguration<ChannelToken>
    {
        public void Configure(EntityTypeBuilder<ChannelToken> builder)
        {
            builder.ToTable("ChannelToken");

            builder.HasKey(k => k.ChannelId);

            builder.Property(p => p.GuildId);
            builder.Property(p => p.Token).IsRequired();
        }
    }
}