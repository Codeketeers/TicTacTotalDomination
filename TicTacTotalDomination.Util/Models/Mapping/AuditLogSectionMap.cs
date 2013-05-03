using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class AuditLogSectionMap : EntityTypeConfiguration<AuditLogSection>
    {
        public AuditLogSectionMap()
        {
            // Primary Key
            this.HasKey(t => t.SectionId);

            // Properties
            this.Property(t => t.Section)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("AuditLogSection");
            this.Property(t => t.SectionId).HasColumnName("SectionId");
            this.Property(t => t.AuditLogId).HasColumnName("AuditLogId");
            this.Property(t => t.Section).HasColumnName("Section");

            // Relationships
            this.HasRequired(t => t.AuditLog)
                .WithMany(t => t.AuditLogSections)
                .HasForeignKey(d => d.AuditLogId);

        }
    }
}
