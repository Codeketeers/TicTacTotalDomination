using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class MatchMap : EntityTypeConfiguration<Match>
    {
        public MatchMap()
        {
            // Primary Key
            this.HasKey(t => t.MatchId);

            // Properties
            // Table & Column Mappings
            this.ToTable("Match");
            this.Property(t => t.MatchId).HasColumnName("MatchId");
            this.Property(t => t.NumberOfGames).HasColumnName("NumberOfGames");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.WonDate).HasColumnName("WonDate");
            this.Property(t => t.PlayerOneId).HasColumnName("PlayerOneId");
            this.Property(t => t.PlayerTwoId).HasColumnName("PlayerTwoId");
            this.Property(t => t.WinningPlayerId).HasColumnName("WinningPlayerId");

            // Relationships
            this.HasRequired(t => t.Player)
                .WithMany(t => t.Matches)
                .HasForeignKey(d => d.PlayerOneId);
            this.HasRequired(t => t.Player1)
                .WithMany(t => t.Matches1)
                .HasForeignKey(d => d.PlayerTwoId);
            this.HasOptional(t => t.Player2)
                .WithMany(t => t.Matches2)
                .HasForeignKey(d => d.WinningPlayerId);

        }
    }
}
