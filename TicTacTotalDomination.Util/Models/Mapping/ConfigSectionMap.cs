using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class ConfigSectionMap : EntityTypeConfiguration<ConfigSection>
    {
        public ConfigSectionMap()
        {
            // Primary Key
            this.HasKey(t => t.SectionId);

            // Properties
            this.Property(t => t.Section)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("ConfigSection");
            this.Property(t => t.SectionId).HasColumnName("SectionId");
            this.Property(t => t.MatchId).HasColumnName("MatchId");
            this.Property(t => t.Section).HasColumnName("Section");

            // Relationships
            this.HasRequired(t => t.Match)
                .WithMany(t => t.ConfigSections)
                .HasForeignKey(d => d.MatchId);

        }
    }
}
