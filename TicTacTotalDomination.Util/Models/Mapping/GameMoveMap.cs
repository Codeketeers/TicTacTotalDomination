using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class GameMoveMap : EntityTypeConfiguration<GameMove>
    {
        public GameMoveMap()
        {
            // Primary Key
            this.HasKey(t => t.MoveId);

            // Properties
            // Table & Column Mappings
            this.ToTable("GameMove");
            this.Property(t => t.MoveId).HasColumnName("MoveId");
            this.Property(t => t.GameId).HasColumnName("GameId");
            this.Property(t => t.PlayerId).HasColumnName("PlayerId");
            this.Property(t => t.MoveDate).HasColumnName("MoveDate");
            this.Property(t => t.IsSettingPiece).HasColumnName("IsSettingPiece");
            this.Property(t => t.X).HasColumnName("X");
            this.Property(t => t.y).HasColumnName("y");

            // Relationships
            this.HasRequired(t => t.Game)
                .WithMany(t => t.GameMoves)
                .HasForeignKey(d => d.GameId);
            this.HasRequired(t => t.Player)
                .WithMany(t => t.GameMoves)
                .HasForeignKey(d => d.PlayerId);

        }
    }
}
