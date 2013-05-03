using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class AIGameMap : EntityTypeConfiguration<AIGame>
    {
        public AIGameMap()
        {
            // Primary Key
            this.HasKey(t => t.AIGameId);

            // Properties
            // Table & Column Mappings
            this.ToTable("AIGame");
            this.Property(t => t.AIGameId).HasColumnName("AIGameId");
            this.Property(t => t.MatchId).HasColumnName("MatchId");
            this.Property(t => t.GameId).HasColumnName("GameId");
            this.Property(t => t.PlayerId).HasColumnName("PlayerId");
            this.Property(t => t.EvaluatingMove).HasColumnName("EvaluatingMove");

            // Relationships
            this.HasRequired(t => t.Game)
                .WithMany(t => t.AIGames)
                .HasForeignKey(d => d.GameId);
            this.HasOptional(t => t.Match)
                .WithMany(t => t.AIGames)
                .HasForeignKey(d => d.MatchId);
            this.HasRequired(t => t.Player)
                .WithMany(t => t.AIGames)
                .HasForeignKey(d => d.PlayerId);

        }
    }
}
