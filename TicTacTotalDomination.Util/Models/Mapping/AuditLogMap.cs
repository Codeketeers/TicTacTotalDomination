using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TicTacTotalDomination.Util.Models.Mapping
{
    public class AuditLogMap : EntityTypeConfiguration<AuditLog>
    {
        public AuditLogMap()
        {
            // Primary Key
            this.HasKey(t => t.LogId);

            // Properties
            this.Property(t => t.LogType)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Metadata)
                .HasMaxLength(250);

            // Table & Column Mappings
            this.ToTable("AuditLog");
            this.Property(t => t.LogId).HasColumnName("LogId");
            this.Property(t => t.LogType).HasColumnName("LogType");
            this.Property(t => t.LogDateTime).HasColumnName("LogDateTime");
            this.Property(t => t.Metadata).HasColumnName("Metadata");
        }
    }
}
