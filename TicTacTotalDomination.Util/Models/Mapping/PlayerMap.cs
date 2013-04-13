using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class PlayerMap : EntityTypeConfiguration<Player>
    {
        public PlayerMap()
        {
            // Primary Key
            this.HasKey(t => t.PlayerId);

            // Properties
            this.Property(t => t.PlayerName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Player");
            this.Property(t => t.PlayerId).HasColumnName("PlayerId");
            this.Property(t => t.PlayerName).HasColumnName("PlayerName");
        }
    }
}
