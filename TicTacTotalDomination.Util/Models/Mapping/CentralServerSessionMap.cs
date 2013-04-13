using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class CentralServerSessionMap : EntityTypeConfiguration<CentralServerSession>
    {
        public CentralServerSessionMap()
        {
            // Primary Key
            this.HasKey(t => t.CentralServerSessionId);

            // Properties
            // Table & Column Mappings
            this.ToTable("CentralServerSession");
            this.Property(t => t.CentralServerSessionId).HasColumnName("CentralServerSessionId");
            this.Property(t => t.CentralServerGameId).HasColumnName("CentralServerGameId");
            this.Property(t => t.GameId).HasColumnName("GameId");

            // Relationships
            this.HasRequired(t => t.Game)
                .WithMany(t => t.CentralServerSessions)
                .HasForeignKey(d => d.GameId);

        }
    }
}
