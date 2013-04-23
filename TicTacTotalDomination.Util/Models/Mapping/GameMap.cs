using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class GameMap : EntityTypeConfiguration<Game>
    {
        public GameMap()
        {
            // Primary Key
            this.HasKey(t => t.GameId);

            // Properties
            // Table & Column Mappings
            this.ToTable("Game");
            this.Property(t => t.GameId).HasColumnName("GameId");
            this.Property(t => t.MatchId).HasColumnName("MatchId");
            this.Property(t => t.PlayerOneId).HasColumnName("PlayerOneId");
            this.Property(t => t.PlayerTwoId).HasColumnName("PlayerTwoId");
            this.Property(t => t.WinningPlayerId).HasColumnName("WinningPlayerId");
            this.Property(t => t.CurrentPlayerId).HasColumnName("CurrentPlayerId");
            this.Property(t => t.StateDate).HasColumnName("StateDate");
            this.Property(t => t.CreateDate).HasColumnName("CreateDate");
            this.Property(t => t.WonDate).HasColumnName("WonDate");

            // Relationships
            this.HasOptional(t => t.Player)
                .WithMany(t => t.Games)
                .HasForeignKey(d => d.CurrentPlayerId);
            this.HasOptional(t => t.Match)
                .WithMany(t => t.Games)
                .HasForeignKey(d => d.MatchId);
            this.HasRequired(t => t.Player1)
                .WithMany(t => t.Games1)
                .HasForeignKey(d => d.PlayerOneId);
            this.HasRequired(t => t.Player2)
                .WithMany(t => t.Games2)
                .HasForeignKey(d => d.PlayerTwoId);
            this.HasOptional(t => t.Player3)
                .WithMany(t => t.Games3)
                .HasForeignKey(d => d.WinningPlayerId);

        }
    }
}
