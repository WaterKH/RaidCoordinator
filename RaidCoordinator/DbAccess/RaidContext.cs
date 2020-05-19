using Microsoft.EntityFrameworkCore;
using System.IO;

namespace RaidCoordinator
{
    public class RaidContext : DbContext
    {
        public RaidContext(DbContextOptions<RaidContext> options) : base(options)
        {

        }

        public DbSet<ChannelToken> ChannelTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.ApplyConfiguration(new ChannelTokenConfig());
        }
    }
}
